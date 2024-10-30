$ErrorActionPreference = "Stop"

dotnet build -c Release
cargo build --release

if (Test-Path ./local/GDWeave) {
  Remove-Item ./local/GDWeave -Recurse
}
Copy-Item -Path ./GDWeave/bin/Release/net8.0 -Destination ./local/GDWeave/GDWeave/core -Recurse
Copy-Item -Path ./target/release/loader.dll -Destination ./local/GDWeave/winmm.dll

New-Item -Path ./local/GDWeave/GDWeave/mods -ItemType Directory
Write-Output "Copy your mods into this directory (make sure they're in separate folders)." > ./local/GDWeave/GDWeave/mods/README.txt

if (Test-Path ./local/GDWeave.zip) {
  Remove-Item ./local/GDWeave.zip
}

# Thunderstore
if (Test-Path ./thunderstore/GDWeave) {
  Remove-Item ./thunderstore/GDWeave -Recurse
}
Copy-Item -Path ./local/GDWeave/GDWeave -Destination ./thunderstore/GDWeave -Recurse

if (Test-Path ./thunderstore/winmm.dll) {
  Remove-Item ./thunderstore/winmm.dll
}
Copy-Item -Path ./local/GDWeave/winmm.dll -Destination ./thunderstore/winmm.dll

Copy-Item -Path ./README.md -Destination ./thunderstore/README.md

# thunderstore doesn't need the mods directory
Remove-Item ./thunderstore/GDWeave/mods -Recurse