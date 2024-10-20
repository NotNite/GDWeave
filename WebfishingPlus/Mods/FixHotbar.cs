using GDWeave.Godot;
using GDWeave.Godot.Variants;
using GDWeave.Modding;

namespace WebfishingPlus.Mods;

public class FixHotbar : IScriptMod {
    public bool ShouldRun(string path) => path == "res://Scenes/Singletons/playerdata.gdc";

    public IEnumerable<Token> Modify(string path, IEnumerable<Token> tokens) {
        var eraseMatch = new MultiTokenWaiter([
            t => t is IdentifierToken {Name: "inventory"},
            t => t.Type is TokenType.Period,
            t => t is IdentifierToken {Name: "erase"},
            t => t.Type is TokenType.ParenthesisOpen,
            t => t is IdentifierToken {Name: "item"},
            t => t.Type is TokenType.ParenthesisClose,
            t => t.Type is TokenType.Newline
        ]);
        var zeroMatch = new MultiTokenWaiter([
            t => t is IdentifierToken {Name: "hotbar"},
            t => t.Type is TokenType.BracketOpen,
            t => t is IdentifierToken {Name: "key"},
            t => t.Type is TokenType.BracketClose,
            t => t.Type is TokenType.OpAssign,
            t => t is ConstantToken {Value: IntVariant {Value: 0}},
        ]);

        const string replaceVar = "gdweave_replace";

        foreach (var token in tokens) {
            if (zeroMatch.Check(token)) {
                yield return new IdentifierToken(replaceVar);
            } else if (eraseMatch.Check(token)) {
                yield return token;

                // var replaceVar = 0
                yield return new Token(TokenType.PrVar);
                yield return new IdentifierToken(replaceVar);
                yield return new Token(TokenType.OpAssign);
                yield return new ConstantToken(new IntVariant(0));
                yield return new Token(TokenType.Newline, 1);

                // if item:
                yield return new Token(TokenType.CfIf);
                yield return new IdentifierToken("item");
                yield return new Token(TokenType.Colon);
                yield return new Token(TokenType.Newline, 2);

                // for i in inventory:
                yield return new Token(TokenType.CfFor);
                yield return new IdentifierToken("i");
                yield return new Token(TokenType.OpIn);
                yield return new IdentifierToken("inventory");
                yield return new Token(TokenType.Colon);
                yield return new Token(TokenType.Newline, 3);

                // if i["id"] == item["id"]:
                yield return new Token(TokenType.CfIf);
                yield return new IdentifierToken("i");
                yield return new Token(TokenType.BracketOpen);
                yield return new ConstantToken(new StringVariant("id"));
                yield return new Token(TokenType.BracketClose);
                yield return new Token(TokenType.OpEqual);
                yield return new IdentifierToken("item");
                yield return new Token(TokenType.BracketOpen);
                yield return new ConstantToken(new StringVariant("id"));
                yield return new Token(TokenType.BracketClose);
                yield return new Token(TokenType.Colon);
                yield return new Token(TokenType.Newline, 4);

                // replaceVar = i["ref"]
                yield return new IdentifierToken(replaceVar);
                yield return new Token(TokenType.OpAssign);
                yield return new IdentifierToken("i");
                yield return new Token(TokenType.BracketOpen);
                yield return new ConstantToken(new StringVariant("ref"));
                yield return new Token(TokenType.BracketClose);
                yield return new Token(TokenType.Newline, 4);

                yield return new Token(TokenType.CfBreak);
                yield return new Token(TokenType.Newline, 1);
            } else {
                yield return token;
            }
        }
    }
}
