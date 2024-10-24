using System.CommandLine;
using GDWeave.Godot;
using GDWeave.Modding;

var rootCommand = new RootCommand("GDWeave.Dumper");

var parseCommand = new Command("parse", "Parse a script file");
rootCommand.AddCommand(parseCommand);

var outputOption = new Option<string?>(["--output", "-o"], getDefaultValue: () => null);
var codegenOption = new Option<bool>(["--codegen", "-c"], getDefaultValue: () => false);
var pathArgument = new Argument<string>("path", "Path to the script file");

parseCommand.AddOption(outputOption);
parseCommand.AddOption(codegenOption);
parseCommand.AddArgument(pathArgument);

parseCommand.SetHandler((output, codegen, path) => {
        using var file = File.OpenRead(path);
        using var binaryReader = new BinaryReader(file);
        var scriptFile = new GodotScriptFile(binaryReader);

        using var outputStream = output is not null ? File.Create(output) : Console.OpenStandardOutput();
        using var writer = new StreamWriter(outputStream);

        var tokens = ScriptModder.CreateSpecialTokens(scriptFile);

        if (codegen) {
            var generator = new CodeGenerator(tokens, scriptFile.Identifiers);
            generator.Generate(writer);
        } else {
            writer.WriteLine($"Identifiers: {scriptFile.Identifiers.Count}");
            for (var i = 0; i < scriptFile.Identifiers.Count; i++) {
                writer.WriteLine($"  {i}: {scriptFile.Identifiers[i]}");
            }

            writer.WriteLine($"Constants: {scriptFile.Constants.Count}");
            for (var i = 0; i < scriptFile.Constants.Count; i++) {
                writer.WriteLine($"  {i}: {scriptFile.Constants[i]}");
            }

            writer.WriteLine($"Lines: {scriptFile.Lines.Count}");
            for (var i = 0; i < scriptFile.Lines.Count; i++) {
                writer.WriteLine($"  {i}: {scriptFile.Lines[i].Token} {scriptFile.Lines[i].LineCol}");
            }

            writer.WriteLine($"Tokens: {scriptFile.Tokens.Count}");
            for (var i = 0; i < tokens.Count; i++) {
                writer.WriteLine($"  {i}: {tokens[i]}");
            }
        }
    },
    outputOption,
    codegenOption,
    pathArgument
);

await rootCommand.InvokeAsync(args);
