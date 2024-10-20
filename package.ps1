dotnet clean
dotnet build -c Release
cargo build --release

if (Test-Path ./local/GDWeave) {
  Remove-Item ./local/GDWeave -Recurse
}
Copy-Item -Path ./GDWeave/bin/Release/net8.0 -Destination ./local/GDWeave/GDWeave/core -Recurse
Copy-Item -Path ./target/release/loader.dll -Destination ./local/GDWeave/winmm.dll

if (Test-Path ./local/GDWeave.zip) {
  Remove-Item ./local/GDWeave.zip
}
Compress-Archive -Path ./local/GDWeave/* -DestinationPath ./local/GDWeave.zip

if (Test-Path ./local/WebfishingPlus) {
  Remove-Item ./local/WebfishingPlus -Recurse
}
Copy-Item -Path ./WebfishingPlus/bin/Release/net8.0 -Destination ./local/WebfishingPlus -Recurse

if (Test-Path ./local/WebfishingPlus.zip) {
  Remove-Item ./local/WebfishingPlus.zip
}
Compress-Archive -Path ./local/WebfishingPlus/* -DestinationPath ./local/WebfishingPlus.zip
