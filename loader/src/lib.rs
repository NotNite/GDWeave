#![feature(naked_functions)]
#![allow(named_asm_labels)]
#![allow(non_snake_case)]

mod export_indices;
mod intercepted_exports;
mod orig_exports;
mod proxied_exports;

#[allow(unused_imports)]
pub use intercepted_exports::*;
pub use proxied_exports::*;

use export_indices::TOTAL_EXPORTS;
use orig_exports::load_dll_funcs;
#[cfg(target_arch = "x86")]
use std::arch::x86::_mm_pause;
#[cfg(target_arch = "x86_64")]
use std::arch::x86_64::_mm_pause;
use std::os::windows::prelude::AsRawHandle;
use win_msgbox::Okay;
use winapi::ctypes::c_void;
use winapi::shared::minwindef::{FARPROC, HMODULE};
use winapi::shared::ntdef::LPCSTR;
use winapi::um::consoleapi::AllocConsole;
use winapi::um::errhandlingapi::GetLastError;
use winapi::um::libloaderapi::{DisableThreadLibraryCalls, FreeLibrary, LoadLibraryA};
use winapi::um::processenv::SetStdHandle;
use winapi::um::processthreadsapi::{CreateThread, GetCurrentProcess, TerminateProcess};
use winapi::um::winbase::{STD_ERROR_HANDLE, STD_OUTPUT_HANDLE};
use winapi::um::winnt::{DLL_PROCESS_ATTACH, DLL_PROCESS_DETACH};

use isahc::ReadResponseExt;
use netcorehost::{error::HostingError, nethost, pdcstr, pdcstring::PdCString};
use thiserror::Error;
use win_msgbox::YesNo;

#[derive(Error, Debug)]
pub enum LoaderError {
    #[error("Failed to load hostfxr")]
    LoadHostfxrError(#[from] nethost::LoadHostfxrError),

    #[error("Failed to host")]
    HostingError(#[from] netcorehost::error::HostingError),

    #[error("Failed to call init function")]
    InitError(#[from] netcorehost::hostfxr::GetManagedFunctionError),

    #[error("Unable to convert string")]
    CString,

    #[error("IO error")]
    Io(#[from] std::io::Error),

    #[error("Unknown error")]
    Unknown,
}

// Static handles
static mut THIS_HANDLE: Option<HMODULE> = None;
static mut ORIG_DLL_HANDLE: Option<HMODULE> = None;

// Original funcs
#[no_mangle]
static mut ORIGINAL_FUNCS: [FARPROC; TOTAL_EXPORTS] = [std::ptr::null_mut(); TOTAL_EXPORTS];
#[no_mangle]
static mut ORIG_FUNCS_PTR: *const FARPROC = std::ptr::null_mut();

/// Indicates once we are ready to accept incoming calls to proxied functions
static mut PROXYGEN_READY: bool = false;

#[no_mangle]
pub unsafe extern "stdcall" fn DllMain(module: HMODULE, reason: u32, _res: *const c_void) -> i32 {
    DisableThreadLibraryCalls(module);
    THIS_HANDLE = Some(module);

    if reason == DLL_PROCESS_ATTACH {
        CreateThread(
            std::ptr::null_mut(),
            0,
            Some(init),
            std::ptr::null_mut(),
            0,
            std::ptr::null_mut(),
        );
    } else if reason == DLL_PROCESS_DETACH {
        if let Some(orig_dll_handle) = ORIG_DLL_HANDLE {
            println!("Freeing original DLL");
            FreeLibrary(orig_dll_handle);
        }
    }

    1
}

unsafe fn die() {
    win_msgbox::information::<Okay>("Exiting...")
        .title("LOADER")
        .show()
        .ok();
    println!("Exiting...");
    TerminateProcess(GetCurrentProcess(), 0);
}

/// Called when the thread is spawned
unsafe extern "system" fn init(_: *mut c_void) -> u32 {
    ORIG_FUNCS_PTR = ORIGINAL_FUNCS.as_ptr();
    match std::env::var("GDWEAVE_CONSOLE") {
        Ok(_) => {
            AllocConsole();
        }
        Err(_) => {}
    }

    let stdout = std::io::stdout();
    let out_handle = stdout.as_raw_handle();
    let out_handle = out_handle as *mut c_void;
    SetStdHandle(STD_OUTPUT_HANDLE, out_handle);
    let stderr = std::io::stderr();
    let err_handle = stderr.as_raw_handle();
    let err_handle = err_handle as *mut c_void;
    SetStdHandle(STD_ERROR_HANDLE, err_handle);
    ORIG_DLL_HANDLE = Some(LoadLibraryA(
        b"C:\\Windows\\system32\\winmm.dll\0".as_ptr() as LPCSTR
    ));
    if let Some(orig_dll_handle) = ORIG_DLL_HANDLE {
        if orig_dll_handle.is_null() {
            let err = GetLastError();
            eprintln!("Failed to load original DLL");
            win_msgbox::error::<Okay>(&format!("Failed to load original DLL. Error: {}", err))
                .title("LOADER")
                .show()
                .ok();
            die();
        }
        println!("Original DLL handle: {:?}", orig_dll_handle);
    } else {
        let err = GetLastError();
        eprintln!("Failed to load original DLL");
        win_msgbox::error::<Okay>(&format!("Failed to load original DLL. Error: {}", err))
            .title("LOADER")
            .show()
            .ok();
        die();
    }
    load_dll_funcs();
    PROXYGEN_READY = true;
    0
}

/// Call this before attempting to call a function in the proxied DLL
///
/// This will wait for proxygen to fully load up all the proxied function addresses before returning
#[no_mangle]
pub extern "C" fn wait_dll_proxy_init() {
    // NOTE TO SELF: DO NO PRINT STUFF IN HERE

    // Safety: `PROXYGEN_READY` will only get flipped to true once, and never back again.
    // We also check if sse2 is supported before using _mm_pause
    if is_x86_feature_detected!("sse2") {
        unsafe {
            while !PROXYGEN_READY {
                _mm_pause();
            }
        }
    } else {
        while !unsafe { PROXYGEN_READY } {
            std::thread::sleep(std::time::Duration::from_millis(100));
        }
    }
}

fn init_gdweave() -> Result<(), LoaderError> {
    let args = std::env::args().collect::<Vec<_>>();
    if args.iter().any(|s| s == "--gdweave-disable") {
        return Ok(());
    }
    if let Some(i) = args
        .iter()
        .position(|s| s.starts_with("--gdweave-folder-override="))
    {
        let path = args[i].split('=').nth(1).unwrap();
        std::env::set_var("GDWEAVE_FOLDER_OVERRIDE", path);
    }

    let dir = std::env::var("GDWEAVE_FOLDER_OVERRIDE")
        .map(|s| std::path::PathBuf::from(s))
        .unwrap_or_else(|_| {
            std::env::current_exe()
                .unwrap()
                .parent()
                .unwrap()
                .join("GDWeave")
        });
    let core = dir.join("core");
    let runtime_config_path = core.join("GDWeave.runtimeconfig.json");
    let dll_path = core.join("GDWeave.dll");

    let hostfxr = nethost::load_hostfxr()?;
    let runtime_config_pdcstr = PdCString::from_os_str(runtime_config_path.as_os_str())
        .map_err(|_| LoaderError::CString)?;
    let context = hostfxr.initialize_for_runtime_config(runtime_config_pdcstr)?;

    let dll_pdcstr =
        PdCString::from_os_str(dll_path.as_os_str()).map_err(|_| LoaderError::CString)?;
    let loader = context.get_delegate_loader_for_assembly(dll_pdcstr)?;

    loader.get_function::<fn()>(
        pdcstr!("GDWeave.GDWeave, GDWeave"),
        pdcstr!("Main"),
        pdcstr!("GDWeave.GDWeave+MainDelegate, GDWeave"),
    )?();

    Ok(())
}

// https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-8.0.403-windows-x64-installer
const DOTNET_URL: &str = "https://download.visualstudio.microsoft.com/download/pr/6224f00f-08da-4e7f-85b1-00d42c2bb3d3/b775de636b91e023574a0bbc291f705a/dotnet-sdk-8.0.403-win-x64.exe";

fn install_net() -> anyhow::Result<()> {
    let path = std::env::current_exe()?
        .parent()
        .ok_or(LoaderError::Unknown)?
        .join("dotnet-sdk.exe");

    let mut resp = isahc::get(DOTNET_URL)?;
    let bytes = resp.bytes()?;
    std::fs::write(&path, bytes)?;

    std::process::Command::new(&path).spawn()?.wait()?;

    std::fs::remove_file(path)?;

    Ok(())
}

pub fn start() {
    if let Err(e) = init_gdweave() {
        match e {
            LoaderError::LoadHostfxrError(_)
            | LoaderError::HostingError(HostingError::FrameworkMissingFailure) => {
                let should_install_net = win_msgbox::information::<YesNo>("GDWeave couldn't load the .NET Runtime. Would you like to install it?\nDownloading the installer will take a moment. You'll need to restart the game after installation.")
                    .title("GDWeave")
                    .show()
                    .unwrap_or(YesNo::No);

                if should_install_net == YesNo::Yes {
                    std::thread::spawn(|| {
                        if let Err(e) = install_net() {
                            win_msgbox::warning::<Okay>(&format!(
                                "Failed to install .NET:\n{:?}",
                                e
                            ))
                            .title("GDWeave")
                            .show()
                            .ok();
                        }
                    });
                };
            }

            _ => {
                win_msgbox::warning::<Okay>(&format!("GDWeave failed to start:\n{:?}", e))
                    .title("GDWeave")
                    .show()
                    .ok();
            }
        }
    }
}
