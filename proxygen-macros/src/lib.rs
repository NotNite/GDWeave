extern crate proc_macro;
extern crate quote;
extern crate syn;

use proc_macro::TokenStream;
use quote::quote;
use syn::{FnArg, ItemFn};

const GET_ARG_TYPES: fn(&FnArg) -> syn::Ident = |arg: &FnArg| match arg {
    FnArg::Receiver(_) => panic!("Cannot use receivers (self) with proxy functions"),
    FnArg::Typed(arg) => {
        if let syn::Type::Path(syn::TypePath {
            path: syn::Path { segments, .. },
            ..
        }) = arg.ty.as_ref()
        {
            return segments.first().unwrap().ident.clone();
        }
        panic!("Unsupported function signature");
    }
};

const GET_ARG_NAMES: fn(&FnArg) -> syn::Ident = |arg: &FnArg| match arg {
    FnArg::Receiver(_) => panic!("Cannot use receivers (self) with proxy functions"),
    FnArg::Typed(arg) => {
        let syn::PatType { pat, .. } = &arg;
        let pat = pat.clone();
        match *pat {
            syn::Pat::Ident(ident) => ident.ident,
            _ => panic!("Unexpected arg name: {:?}", pat),
        }
    }
};

#[derive(Debug, Clone, Copy)]
enum ProxySignatureType {
    Known,
    Unknown,
}

impl From<syn::Meta> for ProxySignatureType {
    fn from(meta: syn::Meta) -> Self {
        match meta {
            syn::Meta::Path(_) => panic!("Unsupported attribute inputs"),
            syn::Meta::List(_) => panic!("Unsupported attribute inputs"),
            syn::Meta::NameValue(sig) => {
                if let Some(ident) = sig.path.get_ident() {
                    if ident.to_string() != "sig" {
                        panic!("Expected sig=\"unknown\" or sig=\"known\"")
                    }
                    if let syn::Expr::Lit(syn::ExprLit {
                        lit: syn::Lit::Str(token),
                        ..
                    }) = sig.value
                    {
                        match token.value().as_str() {
                            "known" => ProxySignatureType::Known,
                            "unknown" => ProxySignatureType::Unknown,
                            _ => panic!("Expected sig=\"unknown\" or sig=\"known\""),
                        }
                    } else {
                        panic!("Expected sig=\"unknown\" or sig=\"known\"")
                    }
                } else {
                    panic!("Expected sig=\"unknown\" or sig=\"known\"")
                }
            }
        }
    }
}

// Proc macro to forward a function call to the orginal function
//
// Note: You may not have any instructions in the function body when forwarding function calls
#[proc_macro_attribute]
pub fn forward(_attr_input: TokenStream, item: TokenStream) -> TokenStream {
    let input: ItemFn = syn::parse(item).expect("You may only proxy a function");
    let func_name = input.sig.clone().ident;
    let func_body = input.block.stmts.clone();
    let ret_type = input.sig.output.clone();
    let orig_index_ident =
        syn::parse_str::<syn::Path>(&format!("crate::export_indices::Index_{}", &func_name))
            .unwrap();
    let arg_types = input.sig.inputs.iter().map(GET_ARG_TYPES);
    let attrs = input
        .attrs
        .into_iter()
        .filter(|attr| !attr.path().is_ident("proxy"));

    if arg_types.len() > 0 {
        panic!("You may not specifiy arguments in a forwarding proxy");
    }
    match ret_type.clone() {
        syn::ReturnType::Default => {}
        syn::ReturnType::Type(_, ty) => match *ty {
            syn::Type::Path(ref p) => {
                if !p.path.is_ident("()") {
                    panic!("You may not specify a return type when forwarding a function call");
                }
            }
            syn::Type::Tuple(ref t) => {
                if !t.elems.is_empty() {
                    panic!("You may not specify a return type when forwarding a function call");
                }
            }
            _ => panic!("You may not specify a return type when forwarding a function call"),
        },
    };
    if func_body.len() > 0 {
        panic!("Your function body will not get run in a forwarding proxy. Perhaps you meant to use a `pre_hook`?");
    }

    TokenStream::from(quote!(
        #[naked]
        #(#attrs)*
        pub unsafe extern "C" fn #func_name() {
            #[cfg(target_arch = "x86_64")]
            {
                std::arch::naked_asm!(
                    "call {wait_dll_proxy_init}",
                    "mov rax, qword ptr [rip + {ORIG_FUNCS_PTR}]",
                    "add rax, {orig_index} * 8",
                    "mov rax, qword ptr [rax]",
                    "push rax",
                    "ret",
                    wait_dll_proxy_init = sym crate::wait_dll_proxy_init,
                    ORIG_FUNCS_PTR = sym crate::ORIG_FUNCS_PTR,
                    orig_index = const #orig_index_ident,
                    //options(noreturn)
                )
            }

            #[cfg(target_arch = "x86")]
            {
                std::arch::naked_asm!(
                    "call {wait_dll_proxy_init}",
                    "mov eax, dword ptr [{ORIG_FUNCS_PTR}]",
                    "add eax, {orig_index} * 4",
                    "mov eax, dword ptr [eax]",
                    "push eax",
                    "ret",
                    wait_dll_proxy_init = sym crate::wait_dll_proxy_init,
                    ORIG_FUNCS_PTR = sym crate::ORIG_FUNCS_PTR,
                    orig_index = const #orig_index_ident,
                    //options(noreturn)
                )
            }
        }
    ))
}

// Proc macro to bring the original function into the scope of an interceptor function as `orig_func`
#[proc_macro_attribute]
pub fn proxy(attr_input: TokenStream, item: TokenStream) -> TokenStream {
    let input: ItemFn = syn::parse(item).expect("You may only proxy a function");
    let attr_input = syn::parse::<syn::Meta>(attr_input);
    let func_name = input.sig.clone().ident;
    let func_sig = input.sig.clone();
    let func_body = input.block.stmts.clone();
    let ret_type = input.sig.output.clone();
    let orig_index_ident =
        syn::parse_str::<syn::Path>(&format!("crate::export_indices::Index_{}", &func_name))
            .unwrap();
    let arg_types = input.sig.inputs.iter().map(GET_ARG_TYPES);
    let attrs = input
        .attrs
        .into_iter()
        .filter(|attr| !attr.path().is_ident("proxy"));
    let sig_type: ProxySignatureType = match attr_input {
        Ok(attr_input) => attr_input.into(),
        Err(_) => panic!("Please explictly set sig=\"known\" or sig=\"unknown\". Eg. #[post_hook(sig = \"known\")]"),
    };

    match sig_type {
        ProxySignatureType::Known => {
            TokenStream::from(quote!(
                #(#attrs)*
                #func_sig {
                    crate::wait_dll_proxy_init();
                    let orig_func: fn (#(#arg_types,)*) #ret_type = unsafe { std::mem::transmute(crate::ORIGINAL_FUNCS[#orig_index_ident]) };
                    #(#func_body)*
                }
            ))
        },
        ProxySignatureType::Unknown => panic!("You may not manual-proxy a function with an unknown signature (only pre-hooking is supported)"),
    }
}

/// Proc macro that indicates that any code in this function will be run just before the original function is called.
//
// You should explicitly set `sig="known"` if you know the function signature
//
/// The function being proxied can be accessed as `orig_func`
///
/// Note: Returning in this function will skip running the original.
#[proc_macro_attribute]
pub fn pre_hook(attr_input: TokenStream, item: TokenStream) -> TokenStream {
    let input: ItemFn = syn::parse(item).expect("You may only proxy a function");
    let attr_input = syn::parse::<syn::Meta>(attr_input);
    let func_name = input.sig.ident.clone();
    let func_sig = input.sig.clone();
    let func_body = input.block.stmts.clone();
    let ret_type = input.sig.output.clone();
    let orig_index_ident =
        syn::parse_str::<syn::Path>(&format!("crate::export_indices::Index_{}", &func_name))
            .unwrap();
    let arg_names = input.sig.inputs.iter().map(GET_ARG_NAMES);
    let arg_types = input.sig.inputs.iter().map(GET_ARG_TYPES);
    let attrs = input
        .attrs
        .into_iter()
        .filter(|attr| !attr.path().is_ident("pre_hook"));
    let sig_type: ProxySignatureType = match attr_input {
            Ok(attr_input) => attr_input.into(),
            Err(_) => panic!("Please explictly set sig=\"known\" or sig=\"unknown\". Eg. #[post_hook(sig = \"known\")]"),
        };

    match sig_type {
        ProxySignatureType::Known => TokenStream::from(quote!(
            #(#attrs)*
            #func_sig {
                let orig_func: fn (#(#arg_types,)*) #ret_type = unsafe { std::mem::transmute(crate::ORIGINAL_FUNCS[#orig_index_ident]) };
                #(#func_body)*
                orig_func(#(#arg_names,)*)
            }
        )),
        ProxySignatureType::Unknown => {
            if arg_names.clone().len() != 0 {
                panic!("You may not specifiy any arguments when proxying a function with an unknown signature");
            }
            match ret_type.clone() {
                syn::ReturnType::Default => {},
                syn::ReturnType::Type(_, ty) => {
                    match *ty {
                        syn::Type::Path(ref p) => if !p.path.is_ident("()") {
                            panic!("You may not specify a return type when proxying a function with an unknown signature");
                        },
                        syn::Type::Tuple(ref t) => if !t.elems.is_empty() {
                            panic!("You may not specify a return type when proxying a function with an unknown signature");
                        },
                        _ => panic!("You may not specify a return type when proxying a function with an unknown signature")
                    }
                }
            };
            let hook_func_name =
                syn::parse_str::<syn::Ident>(&format!("Proxygen_PreHook_{}", &func_name)).unwrap();
            TokenStream::from(quote!(
                #[cfg(not(target_arch = "x86_64"))]
                compile_error!("Pre-hooks aren't yet implemented for non x86-64");

                #[no_mangle]
                // TODO: Use the same safety/unsafety modifier as the original here
                pub unsafe extern "C" fn #hook_func_name() {
                    let orig_func: fn () = std::mem::transmute(crate::ORIGINAL_FUNCS[#orig_index_ident]);
                    #(#func_body)*
                }

                #[naked]
                #(#attrs)*
                pub unsafe extern "C" fn #func_name() {
                    std::arch::naked_asm!(
                        // Wait for dll proxy to initialize
                        "call {wait_dll_proxy_init}",
                        "mov rax, qword ptr [rip + {ORIG_FUNCS_PTR}]",
                        "add rax, {orig_index} * 8",
                        "mov rax, qword ptr [rax]",

                        // Push the original function onto the stack
                        "push rax",

                        // Save the general purpose registers
                        "push rdi; push rsi; push rcx; push rdx; push r8; push r9",

                        // Save the 128-bit floating point registers
                        "sub rsp, 64",
                        "movaps [rsp], xmm0",
                        "movaps [rsp + 16], xmm1",
                        "movaps [rsp + 32], xmm2",
                        "movaps [rsp + 48], xmm3",

                        // Call our hook code here
                        "call {proxygen_pre_hook_func}",

                        // Restore the 128-bit floating point registers
                        "movaps xmm3, [rsp + 48]",
                        "movaps xmm2, [rsp + 32]",
                        "movaps xmm1, [rsp + 16]",
                        "movaps xmm0, [rsp]",
                        "add rsp, 64",

                        // Restore the general purpose registers
                        "pop r9; pop r8; pop rdx; pop rcx; pop rsi; pop rdi",

                        // Return to the original function
                        "ret",
                        wait_dll_proxy_init = sym crate::wait_dll_proxy_init,
                        ORIG_FUNCS_PTR = sym crate::ORIG_FUNCS_PTR,
                        orig_index = const #orig_index_ident,
                        proxygen_pre_hook_func = sym #hook_func_name,
                        //options(noreturn)
                    );
                }
            ))
        }
    }
}

/// Proc macro that indicates that any code in this function will be run after the original function is called.
///
/// The result of calling the original function will be accessible in `orig_result`.
///
/// Note: `orig_result` will be returned unless you choose to return your own result from this function.
#[proc_macro_attribute]
pub fn post_hook(attr_input: TokenStream, item: TokenStream) -> TokenStream {
    let input: ItemFn = syn::parse(item).expect("You may only proxy a function");
    let attr_input = syn::parse::<syn::Meta>(attr_input);
    let func_name = input.sig.clone().ident;
    let func_sig = input.sig.clone();
    let func_body = input.block.stmts.clone();
    let ret_type = input.sig.output.clone();
    let orig_index_ident =
        syn::parse_str::<syn::Path>(&format!("crate::export_indices::Index_{}", &func_name))
            .unwrap();
    let arg_names = input.sig.inputs.iter().map(GET_ARG_NAMES);
    let arg_types = input.sig.inputs.iter().map(GET_ARG_TYPES);
    let attrs = input
        .attrs
        .into_iter()
        .filter(|attr| !attr.path().is_ident("post_hook"));
    let sig_type: ProxySignatureType = match attr_input {
        Ok(attr_input) => attr_input.into(),
        Err(_) => panic!("Please explictly set sig=\"known\" or sig=\"unknown\". Eg. #[post_hook(sig = \"known\")]"),
    };

    match sig_type {
        ProxySignatureType::Known => TokenStream::from(quote!(
            #(#attrs)*
            #func_sig {
                crate::wait_dll_proxy_init();
                let orig_func: fn (#(#arg_types,)*) #ret_type = unsafe { std::mem::transmute(crate::ORIGINAL_FUNCS[#orig_index_ident]) };
                let orig_result = orig_func(#(#arg_names,)*);
                #(#func_body)*
                orig_result
            }
        )),
        ProxySignatureType::Unknown => {
            panic!("You may not post-hook a function with an unknown signature (only pre-hooking is supported)");
        }
    }
}
