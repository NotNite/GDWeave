using GDWeave.Parser;
using GDWeave.Parser.Variants;

namespace GDWeave.Mods;

public class MenuTweaks : ScriptMod {
    public override bool ShouldRun(string path) => path == "res://Scenes/Menus/Main Menu/main_menu.gdc";

    public override IEnumerable<Token> Modify(string path, IEnumerable<Token> tokens) {
        var versionStringWaiter = new MultiTokenWaiter([
            t => t is ConstantToken {Value: StringVariant {Value: "lamedeveloper v"}},
            t => t is IdentifierToken {Name: "GAME_VERSION"},
            t => t.Type is TokenType.Newline
        ], allowPartialMatch: true);
        var disabledWaiter = new MultiTokenWaiter([
            t => t is IdentifierToken {Name: "HBoxContainer"},
            t => t.Type is TokenType.OpDiv,
            t => t is IdentifierToken {Name: "Button"},
            t => t.Type is TokenType.Period,
            t => t is IdentifierToken {Name: "disabled"},
            t => t.Type is TokenType.OpAssign,
        ]);
        var newlineConsumer = new TokenConsumer(t => t.Type is TokenType.Newline);

        foreach (var token in tokens) {
            if (newlineConsumer.Check(token)) {
                continue;
            } else if (newlineConsumer.Ready) {
                yield return token;
                newlineConsumer.Reset();
            }

            if (versionStringWaiter.Check(token)) {
                // ... + "+ GDWeave v" + GDWeave.VERSION
                yield return new Token(TokenType.OpAdd);
                yield return new ConstantToken(new StringVariant($" + GDWeave {GDWeave.Version}"));
                yield return token;
            } else if (disabledWaiter.Check(token)) {
                yield return token;
                yield return new ConstantToken(new BoolVariant(false));

                // Reset since this matches multiple times
                disabledWaiter.Reset();
                // Consume the "disabled or refreshing" and wait until newline
                newlineConsumer.SetReady();
            } else {
                yield return token;
            }
        }
    }
}
