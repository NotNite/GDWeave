using System.Text;
using GDWeave.Parser.Variants;

namespace GDWeave.Parser;

public class GodotScriptFile {
    public const uint Magic = 0x43534447;   // 'G', 'D', 'S', 'C'
    public const uint Version = 13;         // Godot 3.5.2
    public const byte UselessXorKey = 0xB6; // What

    public List<string> Identifiers = new();
    public List<Variant> Constants = new();
    public List<(uint Token, uint LineCol)> Lines = new();
    public List<Token> Tokens = new();

    public GodotScriptFile(BinaryReader br) {
        var header = br.ReadUInt32();
        if (header != Magic) throw new InvalidDataException("Invalid file header");
        var version = br.ReadUInt32();
        if (version != 13) throw new InvalidDataException("Invalid file version");

        var identifierCount = br.ReadUInt32();
        var constantCount = br.ReadUInt32();
        var lineCount = br.ReadUInt32();
        var tokenCount = br.ReadUInt32();

        for (var i = 0; i < identifierCount; i++) {
            var len = br.ReadUInt32();
            var bytes = br.ReadBytes((int) len);
            for (var j = 0; j < len; j++) bytes[j] ^= UselessXorKey;

            var str = Encoding.UTF8.GetString(bytes).TrimEnd('\0');
            this.Identifiers.Add(str);
        }

        for (var i = 0; i < constantCount; i++) {
            var variant = VariantParser.Read(br)!;
            this.Constants.Add(variant);
        }

        for (var i = 0; i < lineCount; i++) {
            var token = br.ReadUInt32();
            var lineCol = br.ReadUInt32();
            this.Lines.Add((token, lineCol));
        }

        for (var i = 0; i < tokenCount; i++) {
            this.Tokens.Add(TokenParser.Read(br));
        }
    }

    public void Write(BinaryWriter bw) {
        bw.Write(Magic);
        bw.Write(Version);

        bw.Write(this.Identifiers.Count);
        bw.Write(this.Constants.Count);
        bw.Write(this.Lines.Count);
        bw.Write(this.Tokens.Count);

        foreach (var identifier in this.Identifiers) {
            var bytes = Encoding.UTF8.GetBytes(identifier + '\0');
            var extra = 4 - (bytes.Length % 4);
            if (extra == 4) extra = 0;

            for (var j = 0; j < bytes.Length; j++) bytes[j] ^= UselessXorKey;
            bw.Write((uint) (bytes.Length + extra));
            bw.Write(bytes);
            bw.Write(new byte[extra]);
        }

        foreach (var constant in this.Constants) {
            VariantParser.Write(bw, constant);
        }

        foreach (var (token, lineCol) in this.Lines) {
            bw.Write(token);
            bw.Write(lineCol);
        }

        foreach (var token in this.Tokens) {
            TokenParser.Write(bw, token);
        }
    }
}
