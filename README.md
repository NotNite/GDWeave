# GDWeave

GDWeave is a mod loader & runtime script patching for [the Godot Engine](https://github.com/godotengine/godot).

## Installation

Download [the latest release](https://github.com/NotNite/GDWeave/releases/latest/download/GDWeave.zip) and extract it to your game install. You should end up with a `GDWeave` folder and `winmm.dll` next to the game files.

You can also [install from Thunderstore](https://thunderstore.io/c/webfishing/p/NotNet/GDWeave/).

After GDWeave is installed, you can [install/create some mods](https://github.com/NotNite/GDWeave/blob/main/MODS.md)!

## Troubleshooting/tips

- Do not download GDWeave from the "Code" button on the GitHub page. Download it through [GitHub](https://github.com/NotNite/GDWeave/releases/latest/download/GDWeave.zip) or [Thunderstore](https://thunderstore.io/c/webfishing/p/NotNet/GDWeave/).
- You can open the folder the game is installed into with Steam - right click > "Manage" > "Browse local files".
- You may need to install [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-8.0.403-windows-x64-installer) manually, if the installer from GDWeave doesn't work.
- You may also need to install [Microsoft Visual C++ 2015-2022 Redistributables](https://aka.ms/vs/17/release/vc_redist.x64.exe).
- Users of Proton / Steam Play (i.e. those who are on a Steam Deck, a Chromebook, or are otherwise running Steam for Linux) will need to set their Steam launch arguments to `WINEDLLOVERRIDES="winmm=n,b" %command%`.
- If you're using Wine or Proton, and nothing appears when you attempt to start a game with GDWeave, try downloading [this .gz file](https://github.com/binarylandia/wine_bcryptprimitives_dll_mock/releases/download/2024-11-04_16-27-34/bcryptprimitives.dll-2024-11-04_16-27-34.gz), extracting it, renaming the resulting .dll file to `bcryptprimitives.dll`, and then placing it next to the game files.

## Supported versions

Currently, GDWeave only supports one version (for the game [WEBFISHING](https://store.steampowered.com/app/3146520/WEBFISHING/)), but support for more versions can be added.

- GodotSteam 3.5.2

## FAQ

### How?

GDWeave uses a Rust proxy DLL to start a C# library in the target game's address space, then hooks functions in the Godot engine itself. It then parses the GDScript "bytecode" (really a syntax tree) and runs its own processors over it, rebuilding it in place.

### Why not fork Godot?

Because compiling a modified engine for every Godot version isn't feasible, especially when game developers can use their own forks of Godot.

## Credits

GDWeave's logo is U+1F9F5 "Spool of Thread" in [Twemoji](https://github.com/twitter/twemoji/blob/d94f4cf793e6d5ca592aa00f58a88f6a4229ad43/assets/svg/1f9f5.svg?plain=1).
