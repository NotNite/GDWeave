use netcorehost::{nethost, pdcstr, pdcstring::PdCString};
use proxy_dll::proxy;
use std::path::PathBuf;

fn init() -> anyhow::Result<()> {
    // lol
    let dir = PathBuf::from("D:/code/csharp/GDWeave/GDWeave/bin/Debug/net8.0-windows8.0");

    let runtime_config_path = dir.join("GDWeave.runtimeconfig.json");
    let dll_path = dir.join("GDWeave.dll");

    let hostfxr = nethost::load_hostfxr()?;
    let runtime_config_pdcstr = PdCString::from_os_str(runtime_config_path.as_os_str())?;
    let context = hostfxr.initialize_for_runtime_config(runtime_config_pdcstr)?;

    let dll_pdcstr = PdCString::from_os_str(dll_path.as_os_str())?;
    let loader = context.get_delegate_loader_for_assembly(dll_pdcstr)?;

    loader.get_function::<fn()>(
        pdcstr!("GDWeave.GDWeave, GDWeave"),
        pdcstr!("Main"),
        pdcstr!("GDWeave.GDWeave+MainDelegate, GDWeave"),
    )?();

    Ok(())
}

#[proxy]
pub fn main() {
    if let Err(e) = init() {
        msgbox::create(
            "GDWeave error",
            &format!("{:?}", e),
            msgbox::IconType::Error,
        )
        .ok();
    }
}
