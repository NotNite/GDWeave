using GDWeave.Parser;

namespace GDWeave.Mods;

// Code joins don't work for cross region play right now. I messaged the developer about this, but this works as an interim solution
public class FixCodeJoins : ScriptMod {
    public override bool ShouldRun(string path) => path == "res://Scenes/Singletons/SteamNetwork.gdc";

    public override IEnumerable<Token> Modify(string path, IEnumerable<Token> tokens) {
        var waitingForNewline = false;
        var injected = false;

        foreach (var t in tokens) {
            if (t is IdentifierToken {Name: "_search_for_lobby"} && !injected) {
                waitingForNewline = true;
            }

            yield return t;

            if (waitingForNewline && t.Type is TokenType.Newline) {
                yield return new IdentifierToken("Steam");
                yield return new Token(TokenType.Period);
                yield return new IdentifierToken("addRequestLobbyListDistanceFilter");
                yield return new Token(TokenType.ParenthesisOpen);
                yield return new IdentifierToken("Steam");
                yield return new Token(TokenType.Period);
                yield return new IdentifierToken("LOBBY_DISTANCE_FILTER_WORLDWIDE");
                yield return new Token(TokenType.ParenthesisClose);
                yield return new Token(TokenType.Newline, 1);
                waitingForNewline = false;
                injected = true;
            }
        }
    }
}
