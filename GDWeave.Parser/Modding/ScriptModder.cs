using System.Diagnostics;

namespace GDWeave.Parser;

public class ScriptModder(List<ScriptMod>? mods = null) {
    public void Run(GodotScriptFile file, string path) {
        var hls = new List<Token>();
        foreach (var token in file.Tokens) {
            var hl =
                token.Type switch {
                    TokenType.Identifier when token.AssociatedData is not null => new IdentifierToken {
                        Type = token.Type,
                        AssociatedData = token.AssociatedData,
                        Name = file.Identifiers[(int) token.AssociatedData!.Value]
                    },
                    TokenType.Constant when token.AssociatedData is not null => new ConstantToken {
                        Type = token.Type,
                        AssociatedData = token.AssociatedData,
                        Value = (Variant) file.Constants[(int) token.AssociatedData!.Value].Clone()
                    },
                    _ => token
                };

            hls.Add(hl);
        }

        if (mods != null) {
            foreach (var mod in mods) {
                if (mod.ShouldRun(path)) mod.Modify(path, hls);
            }
        }

        // Translate high level tokens back into tokens
        file.Tokens.Clear();
        foreach (var hl in hls) {
            switch (hl) {
                case ConstantToken constant: {
                    var index = file.Constants.FindIndex(x => x.Equals(constant.Value));
                    if (index == -1) {
                        index = file.Constants.Count;
                        file.Constants.Add(constant.Value);
                    }

                    constant.AssociatedData = (uint) index;
                    break;
                }

                case IdentifierToken identifier: {
                    var index = file.Identifiers.FindIndex(x => x == identifier.Name);
                    if (index == -1) {
                        index = file.Identifiers.Count;
                        file.Identifiers.Add(identifier.Name);
                    }

                    identifier.AssociatedData = (uint) index;
                    break;
                }
            }

            file.Tokens.Add(hl);
        }

        var gameDir = Path.GetDirectoryName(Environment.ProcessPath)!;
        var outFile = Path.Combine(gameDir, "gdc", path.Replace("res://", ""));
        var outDir = Path.GetDirectoryName(outFile)!;
        if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);
        if (File.Exists(outFile)) File.Delete(outFile);
        using var f = File.OpenWrite(outFile);
        using var bw = new BinaryWriter(f);
        file.Write(bw);
    }
}
