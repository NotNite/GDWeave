# GDWeave

GDWeave is an experimental runtime patching system for compiled GDScript bytecode. It currently targets Godot 3.5.2 (for the game [WEBFISHING](https://store.steampowered.com/app/3146520/WEBFISHING/)).

## Installation

- Download [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).
  - If you're playing on Linux, this needs to be installed in the Wine/Proton prefix. The loader should prompt you to do it automatically.
- Download [the latest release](https://github.com/NotNite/GDWeave/releases) and extract it to your game install.
- Start the game.
  - If you're playing on Linux, add `WINEDLLOVERRIDES="winmm=n,b" %command%` to the Steam launch options.

See [here](MODS.md) for installing/creating mods.

## Why?

[GDScript Mod Loader](https://github.com/GodotModding/godot-mod-loader) is bad:

- If your target game doesn't have the mod loader integrated, you must add it yourself. This involves decompiling the game, inserting their own code into the project, rebuilding it, and using that built copy.
- Asset and build sharing is legally questionable, so non-technical users must go through this process.
- Advanced patching is not possible with script extensions.

Games shouldn't need to be fully decompiled into project files to be modded, and game developers shouldn't need to be concerned with their modding community. [Several](https://docs.bepinex.dev/index.html) [modding](https://dev.epicgames.com/documentation/en-us/unreal-engine/plugins-in-unreal-engine) [communities](https://github.com/AurieFramework/YYToolkit) [already](https://reloaded-project.github.io/Reloaded-II/) [acknowledge](https://fabricmc.net/) [this](https://goatcorp.github.io/). So why can't Godot?

## How?

GDWeave uses a Rust proxy DLL to start a C# library in the target game's address space, then hooks functions in the Godot engine itself. It then parses the GDScript "bytecode" (really a syntax tree) and runs its own processors over it, rebuilding it in place.

## Why not fork Godot?

Because compiling a modified engine for every Godot version isn't feasible, especially when game developers can use their own forks of Godot.
