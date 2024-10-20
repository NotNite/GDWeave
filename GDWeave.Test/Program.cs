using GDWeave;
using GDWeave.Godot;
using GDWeave.Modding;

if (args is ["parse", _]) {
    using var file = File.OpenRead(args[1]);
    using var br = new BinaryReader(file);
    var gdsc = new GodotScriptFile(br);

    Console.WriteLine($"Identifiers: {gdsc.Identifiers.Count}");
    for (var i = 0; i < gdsc.Identifiers.Count; i++) {
        Console.WriteLine($"  {i}: {gdsc.Identifiers[i]}");
    }

    Console.WriteLine($"Constants: {gdsc.Constants.Count}");
    for (var i = 0; i < gdsc.Constants.Count; i++) {
        Console.WriteLine($"  {i}: {gdsc.Constants[i]}");
    }

    Console.WriteLine($"Lines: {gdsc.Lines.Count}");
    for (var i = 0; i < gdsc.Lines.Count; i++) {
        Console.WriteLine($"  {i}: {gdsc.Lines[i].Token} {gdsc.Lines[i].LineCol}");
    }

    Console.WriteLine($"Tokens: {gdsc.Tokens.Count}");
    var tokens = ScriptModder.CreateSpecialTokens(gdsc);
    for (var i = 0; i < tokens.Count; i++) {
        Console.WriteLine($"  {i}: {tokens[i]}");
    }
}
