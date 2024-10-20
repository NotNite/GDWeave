if (Test-Path ./local/GDWeave) {
  Remove-Item ./local/GDWeave -Recurse
}
Copy-Item -Path ./GDWeave/bin/Release/net8.0 -Destination ./local/GDWeave/GDWeave/core -Recurse
Copy-Item -Path ./target/release/loader.dll -Destination ./local/GDWeave/winmm.dll

if (Test-Path ./local/GDWeave.zip) {
  Remove-Item ./local/GDWeave.zip
}
Compress-Archive -Path ./local/GDWeave/* -DestinationPath ./local/GDWeave.zip
