using System.CommandLine;
using GDWeave.Godot;
using GDWeave.Modding;

var rootCommand = new RootCommand("GDWeave.Dumper");

// Common options
var pathArgument = new Argument<string>("path", "Path to the script file");

var parseCommand = new Command("parse", "Parse a script file");
rootCommand.AddCommand(parseCommand);

var outputOption = new Option<string?>(["--output", "-o"], getDefaultValue: () => null);
var codegenOption = new Option<bool>(["--codegen", "-c"], getDefaultValue: () => false);
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

var roundtripCommand = new Command("roundtrip", "Test roundtrip serialization");
rootCommand.AddCommand(roundtripCommand);

var tripsOption = new Option<int>(["--trips", "-t"], getDefaultValue: () => 1);
roundtripCommand.AddOption(tripsOption);
roundtripCommand.AddArgument(pathArgument);

roundtripCommand.SetHandler((trips, path) => {
        using var file = File.OpenRead(path);
        using var binaryReader = new BinaryReader(file);
        var scriptFile = new GodotScriptFile(binaryReader);

        using var stream = new MemoryStream();
        using var binaryWriter = new BinaryWriter(stream);

        scriptFile.Write(binaryWriter);


        stream.Seek(0, SeekOrigin.Begin);
        file.Seek(0, SeekOrigin.Begin);

        if (stream.Length != file.Length) {
            throw new Exception($"Length mismatch: expected {file.Length}, got {stream.Length}");
        }

        for (var i = 0; i < stream.Length; i++) {
            var expected = file.ReadByte();
            var actual = stream.ReadByte();
            if (expected != actual) {
                throw new Exception($"Byte mismatch at {i}: expected {expected}, got {actual}");
            }
        }
    },
    tripsOption,
    pathArgument
);

await rootCommand.InvokeAsync(args);
