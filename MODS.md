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
- `Metadata` (optional): object with information about your mod
  - `Name`: a proper name for your mod
  - `Author`: you!
  - `Version`: mod version (keep in sync with your C# assembly, preferably)
  - `Description`: a longer description about your mod

Continue to the following sections for what you need to do:

- [Adding new scripts or assets](#making-a-godot-project) (GDScript)
- [Modifying existing game scripts](#writing-script-mods) (C#)

### Making a Godot project

You can create a new project in the Godot editor (with the same version as your game) and export a .pck. GDWeave will load the script file `res://mods/ModID/main.gd` close to startup (see [technical info](#technical-info) for more information).

After exporting the .pck file in your mod directory, set `PackPath` in the manifest:

```json
{
  "Id": "ModId",
  "PackPath": "ModId.pck"
}
```

It is suggested to work in a separate project, but if you wish to work in a decompiled project, ensure that no game assets are exported in your .pck.

### Writing script mods

Script mods are written in C# using enumerators. Scripts run top-to-bottom, operating on each token of the script. If you are unfamiliar with C# enumerators, consider [reading the documentation](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/yield) for how it works.

A sample project is available [here](https://github.com/NotNite/GDWeave.Sample). After building the assembly, set `AssemblyPath` in the manifest:

```json
{
  "Id": "ModId",
  "AssemblyPath": "ModId.dll"
}
```

You must register script mods through the mod interface:

```cs
modInterface.RegisterScriptMod(new MyScriptMod());
```

For example, if you wanted to wait until the newline after the `extends` keyword:

```cs
public class MyScriptMod : IScriptMod {
    public bool ShouldRun(string path) => path == "res://path/to/script.gdc";

    // returns a list of tokens for the new script, with the input being the original script's tokens
    public IEnumerable<Token> Modify(string path, IEnumerable<Token> tokens) {
        // wait for any newline token after any extends token
        var waiter = new MultiTokenWaiter([
            t => t.Type is TokenType.PrExtends,
            t => t.Type is TokenType.Newline
        ], allowPartialMatch: true);

        // loop through all tokens in the script
        foreach (var token in tokens) {
            if (waiter.Check(token)) {
                // found our match, return the original newline
                yield return token;

                // then add our own code
                yield return new Token(TokenType.BuiltInFunc, (uint?) BuiltinFunction.TextPrint);
                yield return new Token(TokenType.ParenthesisOpen);
                yield return new ConstantToken(new StringVariant("Hello, world!"));
                yield return new Token(TokenType.ParenthesisClose);
                yield return new Token(TokenType.Newline);
            } else {
                // return the original token
                yield return token;
            }
        }
    }
}
```

`yield return` adds a new token to the result, so in this example we return all tokens but make sure to add our own code right below the newline after the `extends` keyword.

There are several helper classes:

- `TokenWaiter`/`MultiTokenWaiter`: wait for certain tokens to be passed through (e.g. adding extra code at a certain point in the script)
- `TokenConsumer`: skips tokens until a certain token is found (e.g. removing the rest of a line)
- `ScriptTokenizer`: turn arbitrary GDScript into a list of tokens (e.g. making it easier to write patches)
  - This class is unstable and may have issues - please report any problems

### Technical info

- Pack files are loaded with an injected script in the first autoload. This is unstable and this behavior may change in the future.
- Mods exist in `/root/ModID` with a `gdweave_mod` group.
- For script mods, GDWeave will load the specified assembly and create the first class that inherits `IMod`. The constructor can take an optional `IModInterface`.

## Publishing to Thunderstore

GDWeave [is on Thunderstore](https://thunderstore.io/c/webfishing/p/NotNet/GDWeave/). Your .zip should look like:

```text
GDWeave/
  mods/
    <mod ID>/
      manifest.json # GDWeave manifest
      <mod files>
manifest.json       # Thunderstore manifest
icon.png
README.md
```

See [here](https://thunderstore.io/c/webfishing/create/docs/) for the Thunderstore package docs. **Do not confuse the two manifests.** Mark GDWeave as a dependency.

## Useful tools

### GDRETools

You can use [GDRETools](https://github.com/bruvzg/gdsdecomp) to decompile the game into a usable project that can be opened in the Godot editor. The menu to decompile the game is in "RE Tools" > "Recover project", then select "Full Recovery".

### GodotSteam

[GodotSteam](https://godotsteam.com/) is a fork of Godot that adds Steamworks support. Most Godot games published on Steam use GodotSteam. If you are working with a game compiled with GodotSteam, use the GodotSteam editor for the appropriate engine version.

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
