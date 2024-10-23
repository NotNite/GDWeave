using GDWeave.Godot;
using GDWeave.Modding;

string path;
if (args.Count() == 0)
{
    Console.WriteLine("Please input a script path");
    return;
}

path = args[0];

bool shouldLog = false;
bool shouldGenCode = false;
if (args.Contains("log", StringComparer.OrdinalIgnoreCase))
{
    shouldLog = true;
}

if (args.Contains("codegen", StringComparer.OrdinalIgnoreCase))
{
    shouldGenCode = true;
}

if (!File.Exists(path))
{
    Console.WriteLine($"Path '{path}' does not exist");
    return;
}

using Stream file = File.OpenRead(path);
using BinaryReader binaryReader = new(file);
GodotScriptFile scriptFile = new(binaryReader);

Stream output;
if (shouldLog)
{
    output = File.Create("./dump.log");
}
else
{
    output = Console.OpenStandardOutput();
}

using StreamWriter writer = new(output);

writer.WriteLine($"Identifiers: {scriptFile.Identifiers.Count}");
for (int i = 0; i < scriptFile.Identifiers.Count; i++)
{
    writer.WriteLine($"  {i}: {scriptFile.Identifiers[i]}");
}

writer.WriteLine($"Constants: {scriptFile.Constants.Count}");
for (int i = 0; i < scriptFile.Constants.Count; i++)
{
    writer.WriteLine($"  {i}: {scriptFile.Constants[i]}");
}

writer.WriteLine($"Lines: {scriptFile.Lines.Count}");
for (int i = 0; i < scriptFile.Lines.Count; i++)
{
    writer.WriteLine($"  {i}: {scriptFile.Lines[i].Token} {scriptFile.Lines[i].LineCol}");
}

writer.WriteLine($"Tokens: {scriptFile.Tokens.Count}");
List<Token> tokens = ScriptModder.CreateSpecialTokens(scriptFile);
for (int i = 0; i < tokens.Count; i++)
{
    writer.WriteLine($"  {i}: {tokens[i]}");
}

if (shouldGenCode)
{
    CodeGenerator generator = new(tokens, scriptFile.Identifiers);
    generator.Generate();
}
