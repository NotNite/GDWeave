using GDWeave.Parser;
using GDWeave.Parser.Variants;

namespace GDWeave.Mods;

public class MenuTweaks : ScriptMod {
    public override bool ShouldRun(string path) => path == "res://Scenes/Menus/Main Menu/main_menu.gdc";

    public override IEnumerable<Token> Modify(string path, IEnumerable<Token> tokens) {
        var versionStringWaiter = new TokenWaiter(
            t => t.Type is TokenType.Newline && t.AssociatedData is 1,
            waitForReady: true
        );
        var isVersionLine = false;

        foreach (var token in tokens) {
            if (token is ConstantToken {Value: StringVariant {Value: "lamedeveloper v"}}) isVersionLine = true;
            if (isVersionLine) {
                if (token is IdentifierToken {Name: "GAME_VERSION"}) {
                    versionStringWaiter.SetReady();
                } else if (token.Type is TokenType.Newline) {
                    isVersionLine = false;
                }
            }

            if (versionStringWaiter.Check(token)) {
                // ... + "+ GDWeave v" + GDWeave.VERSION
                yield return new Token(TokenType.OpAdd);
                yield return new ConstantToken(new StringVariant($" + GDWeave v{GDWeave.Version}"));
                yield return new Token(TokenType.Newline, 1);
            } else {
                yield return token;
            }
        }

        // TODO:
        // Make the start lobby and join from code buttons always enabled
    }
}
