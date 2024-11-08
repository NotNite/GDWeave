using GDWeave.Godot;

namespace GDWeave.Modding;

public class ScriptModder(List<IScriptMod>? mods = null) {
    public bool Run(GodotScriptFile file, string path) {
        var tokens = CreateSpecialTokens(file);

        var ran = false;
        if (mods != null) {
            foreach (var mod in mods) {
                if (mod.ShouldRun(path)) {
                    ran = true;
                    tokens = mod.Modify(path, tokens).ToList();
                }
            }
        }

        // If we have no reason to run, return false
        // Saves us time + less chance for script corruption
        if (!ran) return false;

        // Translate special tokens back into tokens
        file.Tokens.Clear();
        file.Lines.Clear();
        foreach (var token in tokens) {
            switch (token) {
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

            file.Tokens.Add(token);
            var idx = (uint) file.Tokens.Count - 1;
            file.Lines.Add((idx, idx & GodotScriptFile.TokenLineMask));
        }

        return true;
    }

    public static List<Token> CreateSpecialTokens(GodotScriptFile file) {
        var hls = new List<Token>();
        foreach (var token in file.Tokens) {
            var hl =
                token.Type switch {
                    TokenType.Identifier when token.AssociatedData is not null => new IdentifierToken(
                        token.Type,
                        token.AssociatedData,
                        file.Identifiers[(int) (token.AssociatedData ?? 0)]
                    ),
                    TokenType.Constant => new ConstantToken(
                        token.Type,
                        token.AssociatedData,
                        (Variant) file.Constants[(int) (token.AssociatedData ?? 0)].Clone()
                    ),
                    _ => token
                };

            hls.Add(hl);
        }

        return hls;
    }
}
