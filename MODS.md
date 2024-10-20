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

For script manipulation, GDWeave will load the specified assembly and create the first class that inherits `IMod`. The constructor can take an optional `IModInterface`.
