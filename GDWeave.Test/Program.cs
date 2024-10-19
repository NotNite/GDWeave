using GDWeave.Parser;

if (args.Length == 2 && args[0] == "parse") {
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
    var hls = Utils.CreateHighLevel(gdsc);
    for (var i = 0; i < hls.Count; i++) {
        Console.WriteLine($"  {i}: {hls[i]}");
    }

    return;
}

// Lol sorry
const string inputPath = "D:/gdsc.bin";
const string outputPath = "D:/gdsc_out.bin";

{
    if (File.Exists(outputPath)) File.Delete(outputPath);

    using var input = File.OpenRead(inputPath);
    using var br = new BinaryReader(input);
    var gdsc = new GodotScriptFile(br);

    var modder = new ScriptModder();
    modder.Run(gdsc, "");

    using var output = File.OpenWrite(outputPath);
    using var bw = new BinaryWriter(output);

    gdsc.Write(bw);
}

{
    var bytesIn = File.ReadAllBytes(inputPath);
    var bytesOut = File.ReadAllBytes(outputPath);

    if (bytesIn.Length != bytesOut.Length) {
        Console.WriteLine($"Lengths differ: {bytesIn.Length} != {bytesOut.Length}");
        return;
    }

    for (var i = 0; i < bytesIn.Length; i++) {
        if (bytesIn[i] != bytesOut[i]) {
            Console.WriteLine($"Bytes differ at {i}: {bytesIn[i]} != {bytesOut[i]}");
            return;
        }
    }
}
