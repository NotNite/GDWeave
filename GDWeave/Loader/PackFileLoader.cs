using GDWeave.Godot;
using GDWeave.Godot.Variants;
using GDWeave.Modding;

namespace GDWeave;

internal class PackFileLoader(List<LoadedMod> mods) : IScriptMod {
    public bool Ran = false;

    public bool ShouldRun(string path) => !Ran;

    public IEnumerable<Token> Modify(string path, IEnumerable<Token> tokens) {
        var readyWaiter = new MultiTokenWaiter([
            t => t.Type is TokenType.PrFunction,
            t => t is IdentifierToken {Name: "_ready"},
            t => t.Type is TokenType.ParenthesisOpen,
            t => t.Type is TokenType.ParenthesisClose,
            t => t.Type is TokenType.Colon,
            t => t.Type is TokenType.Newline
        ]);

        foreach (var token in tokens) {
            if (readyWaiter.Check(token)) {
                yield return token;

                foreach (var mod in mods) {
                    if (mod.PackPath is null) continue;

                    // ProjectSettings.load_resource_pack(mod.PackPath)
                    yield return new IdentifierToken("ProjectSettings");
                    yield return new Token(TokenType.Period);
                    yield return new IdentifierToken("load_resource_pack");
                    yield return new Token(TokenType.ParenthesisOpen);
                    yield return new ConstantToken(new StringVariant(mod.PackPath));
                    yield return new Token(TokenType.ParenthesisClose);
                    yield return token;

                    // var mod.Manifest.Id = load("res://mods/mod.Manifest.Id/main.gd").new()
                    yield return new Token(TokenType.PrVar);
                    yield return new IdentifierToken($"{mod.Manifest.Id}");
                    yield return new Token(TokenType.OpAssign);
                    yield return new Token(TokenType.BuiltInFunc, (uint?) BuiltinFunction.ResourceLoad);
                    yield return new Token(TokenType.ParenthesisOpen);
                    yield return new ConstantToken(new StringVariant($"res://mods/{mod.Manifest.Id}/main.gd"));
                    yield return new Token(TokenType.ParenthesisClose);
                    yield return new Token(TokenType.Period);
                    yield return new IdentifierToken("new");
                    yield return new Token(TokenType.ParenthesisOpen);
                    yield return new Token(TokenType.ParenthesisClose);
                    yield return token;

                    // mod.Manifest.Id.add_to_group("weave_mod")
                    yield return new IdentifierToken($"{mod.Manifest.Id}");
                    yield return new Token(TokenType.Period);
                    yield return new IdentifierToken("add_to_group");
                    yield return new Token(TokenType.ParenthesisOpen);
                    yield return new ConstantToken(new StringVariant("weave_mod"));
                    yield return new Token(TokenType.ParenthesisClose);
                    yield return token;

                    // mod.Manifest.Id.set_name("mod.Manifest.Id")
                    yield return new IdentifierToken($"{mod.Manifest.Id}");
                    yield return new Token(TokenType.Period);
                    yield return new IdentifierToken("set_name");
                    yield return new Token(TokenType.ParenthesisOpen);
                    yield return new ConstantToken(new StringVariant($"{mod.Manifest.Id}"));
                    yield return new Token(TokenType.ParenthesisClose);
                    yield return token;

                    // get_tree().get_root().call_deferred("add_child", mod.Manifest.Id)
                    yield return new IdentifierToken("get_tree");
                    yield return new Token(TokenType.ParenthesisOpen);
                    yield return new Token(TokenType.ParenthesisClose);
                    yield return new Token(TokenType.Period);
                    yield return new IdentifierToken("get_root");
                    yield return new Token(TokenType.ParenthesisOpen);
                    yield return new Token(TokenType.ParenthesisClose);
                    yield return new Token(TokenType.Period);
                    yield return new IdentifierToken("call_deferred");
                    yield return new Token(TokenType.ParenthesisOpen);
                    yield return new ConstantToken(new StringVariant("add_child"));
                    yield return new Token(TokenType.Comma);
                    yield return new IdentifierToken($"{mod.Manifest.Id}");
                    yield return new Token(TokenType.ParenthesisClose);
                    yield return token;
                }

                Ran = true;
            } else {
                yield return token;
            }
        }
    }
}
