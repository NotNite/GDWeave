namespace GDWeave.Parser;

public class Utils {
    public static void TrimNullTerminator(ref string str) {
        var @null = str.IndexOf('\u0000');
        if (@null != -1) str = str[..@null];
    }

    public static List<Token> CreateHighLevel(GodotScriptFile file) {
        var hls = new List<Token>();
        foreach (var token in file.Tokens) {
            var hl =
                token.Type switch {
                    TokenType.Identifier when token.AssociatedData is not null => new IdentifierToken(
                        token.Type,
                        token.AssociatedData,
                        file.Identifiers[(int) token.AssociatedData!.Value]
                    ),
                    TokenType.Constant when token.AssociatedData is not null => new ConstantToken(
                        token.Type,
                        token.AssociatedData,
                        (Variant) file.Constants[(int) token.AssociatedData!.Value].Clone()
                    ),
                    _ => token
                };

            hls.Add(hl);
        }

        return hls;
    }
}
