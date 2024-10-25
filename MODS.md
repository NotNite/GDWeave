# GDWeave Mods

## Installing mods

Mods go in the `GDWeave/mods` folder, in a folder with the mod name. For example:

```text
(game files)
winmm.dll
GDWeave/
  core/
  mods/
    WebfishingPlus/
      manifest.json
      WebfishingPlus.dll
```

The folder name must match the ID in the manifest.

## Making mods

Create a manifest.json with the following keys:

- `Id`: a unique ID for your mod
- `AssemblyPath` (optional): path to a C# assembly if one exists
- `PackPath` (optional): path to a Godot .pck file if one exists
- `Dependencies` (optional): list of other mod IDs that are required

For assets, GDWeave will load the specified pack file and execute `res://mods/<mod id>/main.gd`, where the mod ID is the ID specified in the manifest.

For script manipulation, GDWeave will load the specified assembly and create the first class that inherits `IMod`. The constructor can take an optional `IModInterface`. You can see a sample C# mod [here](https://github.com/NotNite/GDWeave.Sample).

C# modding is only required if you wish to patch existing scripts. Custom scripts and assets can be done purely in pack files. If working from a decompiled game project, specify include/exclude filters when exporting a .pck to only export the changed files and the `mods` directory.

## Useful tools

### Environment variables

You can set multiple environment variables for debugging information:

- `GDWEAVE_DEBUG` logs more debug information to the log file.
- `GDWEAVE_CONSOLE` opens a console window.
  - You can also start the game from the command line, but this console includes GDWeave output.
- `GDWEAVE_DUMP_GDSC` will output game scripts to a `gdc` folder in the game install. You can [disassemble these scripts](#disassembling-scripts).

Set these environment variables to any value (e.g. `GDWEAVE_DEBUG=1`) to enable them.

### Disassembling scripts

You can use `GDWeave.Dumper` to disassemble a script:

```shell
dotnet run --project ./GDWeave.Dumper -- parse /path/to/script.gdc
```

This will show you the list of tokens used in that script, as well as some other information.

You can specify the following options:

- `--output=<file>`: Write the output to a file instead of standard output.
- `--codegen`: Generates an approximatation of the GDScript source code.
