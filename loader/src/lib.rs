use isahc::ReadResponseExt;
use netcorehost::{error::HostingError, nethost, pdcstr, pdcstring::PdCString};
use proxy_dll::proxy;
use retour::static_detour;
use thiserror::Error;
use win_msgbox::{Okay, YesNo};
use windows::{
    core::PCWSTR,
    Win32::System::{
        LibraryLoader::GetModuleHandleW,
        ProcessStatus::{GetModuleInformation, MODULEINFO},
        Threading::GetCurrentProcess,
    },
};

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

fn init() -> Result<(), LoaderError> {
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

static_detour! {
  static MainHook: fn() -> i32;
}

pub fn main_detour() -> i32 {
    if let Err(e) = init() {
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

    MainHook.call()
}

#[proxy]
pub fn main() {
    unsafe {
        let process = GetCurrentProcess();
        let module = GetModuleHandleW(PCWSTR::null()).unwrap();
        let mut lpmodinfo = MODULEINFO::default();
        GetModuleInformation(
            process,
            module,
            &mut lpmodinfo,
            size_of::<MODULEINFO>() as u32,
        )
        .unwrap();

        let entry = lpmodinfo.EntryPoint;

        MainHook
            .initialize(std::mem::transmute(entry), main_detour)
            .unwrap();
        MainHook.enable().unwrap();
    }
}
