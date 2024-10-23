# GDWeave Dumper
Used to dump GDC bytecode

## How to use
First start the game with the `GDWEAVE_DUMP_GDSC` enviroment variable defined to dump the scripts

Then use `dotnet run /path/to/script`

To log the dump to a file added the `log` switch
`dotnet run /path/to/script log`

To generate "useable" godot script source add the `codegen` switch
`dotnet run /path/to/script codegen`

## Godot Script Source Generator
The generator needs work but for the time being generates readable code. It really struggles with indenting
