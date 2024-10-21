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
