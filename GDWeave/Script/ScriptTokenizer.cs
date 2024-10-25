using System.Text;
using GDWeave.Godot;
using GDWeave.Godot.Variants;

namespace GDWeave.Modding;

public static class ScriptTokenizer {
    private readonly static Dictionary<string, TokenType> Tokens = new() {
        {"continue", TokenType.CfContinue},
        {"return", TokenType.CfReturn},
        {"break", TokenType.CfBreak},
        {"match", TokenType.CfMatch},
        {"while", TokenType.CfWhile},
        {"elif", TokenType.CfElif},
        {"else", TokenType.CfElse},
        {"pass", TokenType.CfPass},
        {"for", TokenType.CfFor},
        {"if", TokenType.CfIf},
        {"const", TokenType.PrConst},
        {"var", TokenType.PrVar},
        {"func", TokenType.PrFunction},
        {"class", TokenType.PrClass},
        {"extends", TokenType.PrExtends},
        {"is", TokenType.PrIs},
        {"as", TokenType.PrAs},
        {"@onready", TokenType.PrOnready},
        {"@tool", TokenType.PrTool},
        {"@export", TokenType.PrExport},

        {"setget", TokenType.PrSetget},
        {"static", TokenType.PrStatic},

        {"void", TokenType.PrVoid},
        {"enum", TokenType.PrEnum},

        {"preload", TokenType.PrPreload},
        {"assert", TokenType.PrAssert},

        {"signal", TokenType.PrSignal},
        {"breakpoint", TokenType.PrBreakpoint},

        {"sync", TokenType.PrSync},
        {"remote", TokenType.PrRemote},
        {"master", TokenType.PrMaster},
        {"slave", TokenType.PrSlave},
        {"puppet", TokenType.PrPuppet},

        {"remotesync", TokenType.PrRemotesync},
        {"mastersync", TokenType.PrMastersync},
        {"puppetsync", TokenType.PrPuppetsync},

        {"\n", TokenType.Newline},

        {"PI", TokenType.ConstPi},
        {"TAU", TokenType.ConstTau},
        {"INF", TokenType.ConstInf},
        {"NAN", TokenType.ConstNan},

        {"error", TokenType.Error},
        {"cursor", TokenType.Cursor},


        {"in", TokenType.OpIn},

        {"_", TokenType.Wildcard},

        {"[", TokenType.BracketOpen},
        {"]", TokenType.BracketClose},
        {"{", TokenType.CurlyBracketOpen},
        {"}", TokenType.CurlyBracketOpen},

        {"(", TokenType.ParenthesisOpen},
        {")", TokenType.ParenthesisClose},

        {",", TokenType.Comma},
        {";", TokenType.Semicolon},
        {".", TokenType.Period},
        {"?", TokenType.QuestionMark},
        {":", TokenType.Colon},
        {"$", TokenType.Dollar},
        {"->", TokenType.ForwardArrow},

        {">>=", TokenType.OpAssignShiftRight},
        {"<<=", TokenType.OpAssignShiftLeft},

        {">>", TokenType.OpShiftRight},
        {"<<", TokenType.OpShiftLeft},

        {"==", TokenType.OpEqual},
        {"!=", TokenType.OpNotEqual},
        {"&&", TokenType.OpAnd},
        {"||", TokenType.OpOr},
        {"!", TokenType.OpNot},

        {"+=", TokenType.OpAssignAdd},
        {"-=", TokenType.OpAssignSub},
        {"*=", TokenType.OpAssignMul},
        {"/=", TokenType.OpAssignDiv},
        {"%=", TokenType.OpAssignMod},
        {"&=", TokenType.OpAssignBitAnd},
        {"|=", TokenType.OpAssignBitOr},
        {"^=", TokenType.OpAssignBitXor},

        {"+", TokenType.OpAnd},
        {"-", TokenType.OpSub},
        {"*", TokenType.OpMul},
        {"/", TokenType.OpDiv},
        {"%", TokenType.OpMod},

        {"~", TokenType.OpBitInvert},
        {"&", TokenType.OpBitAnd},
        {"|", TokenType.OpBitOr},
        {"^", TokenType.OpBitXor},

        {"<=", TokenType.OpLessEqual},
        {">=", TokenType.OpGreaterEqual},
        {"<", TokenType.OpLess},
        {">", TokenType.OpGreater},

        {"=", TokenType.OpAssign},
    };

    private static readonly HashSet<string> Symbols = new() {
        "->",

        ">>=",
        "<<=",

        ">>",
        "<<",

        "==",
        "!=",
        "&&",
        "||",
        "!",

        "+=",
        "-=",
        "*=",
        "/=",
        "%=",
        "&=",
        "|=",
        "^=",

        "_",

        "[",
        "]",

        "{",
        "}",

        "(",
        ")",

        ",",
        ";",
        ".",
        "?",
        ":",
        "$",
        "+",
        "-",
        "*",
        "/",
        "%",

        "~",
        "&",
        "|",
        "^",

        "<=",
        ">=",
        "<",
        ">",

        "=",
    };

    public static IEnumerable<Token> Tokenize(string gdScript, uint baseIndent = 0) {
        IEnumerable<string> tokens = SanitizeInput(TokenizeString(gdScript + " "));

        string previous = string.Empty;
        string idName = string.Empty;

        List<Token> toFlush = new(2);
        yield return new Token(TokenType.Newline, baseIndent);
        foreach (string current in tokens) {
            if (current == "\n") {
                goto endAndFlushId;
            }

            if (previous == "\n")
            {
                uint tabCount = uint.Parse(current);
                toFlush.Add(new Token(TokenType.Newline, tabCount + baseIndent));
                goto end;
            }

            if (current == "_") {
                goto end;
            }

            if (previous == "_" && current == ":") {
                toFlush.Add(new Token(TokenType.Wildcard));
                toFlush.Add(new Token(TokenType.Semicolon));
                goto endAndFlushId;
            }
            else if (previous == "_") {
                idName += "_" + current;
                goto end;
            }

            if (Tokens.TryGetValue(current, out TokenType type)) {
                toFlush.Add(new Token(type));
                goto endAndFlushId;
            }

            if (current[0] == '"') {
                toFlush.Add(new ConstantToken(new StringVariant(current.Substring(1, current.Length - 2))));
                goto endAndFlushId;
            }

            if (bool.TryParse(current, out bool resultB))
            {
                toFlush.Add(new ConstantToken(new BoolVariant(resultB)));
                goto endAndFlushId;
            }

            if (long.TryParse(current, out long resultL)) {
                toFlush.Add(new ConstantToken(new IntVariant(resultL)));
                goto endAndFlushId;
            }

            if (double.TryParse(current, out double result)) {
                toFlush.Add(new ConstantToken(new RealVariant(result)));
                goto endAndFlushId;
            }

            idName += current;

            goto end;
endAndFlushId:
            if (idName != string.Empty) {
                yield return new IdentifierToken(idName);
                idName = string.Empty;
            }
end:
            previous = current;
            foreach (Token token in toFlush) yield return token;
            toFlush.Clear();
        }

        yield return new(TokenType.Newline, baseIndent);
    }

    private static IEnumerable<string> SanitizeInput(IEnumerable<string> tokens) {
        foreach (string token in tokens) {
            if (token != "\n" && string.IsNullOrWhiteSpace(token)) {
                continue;
            }

            yield return token;
        }
    }

    private static IEnumerable<string> TokenizeString(string text) {
        StringBuilder builder = new(20);
        for (int i = 0; i < text.Length; i++) {
            if (text[i] == '"') {
                yield return ClearBuilder();
                builder.Append('"');
                i++;
                for (; i < text.Length; i++) {
                    builder.Append(text[i]);
                    if (text[i] == '"') {
                        break;
                    }
                }

                yield return ClearBuilder();
                continue;
            }

            // This is stupid and awful
            if (text[i] == '\n') {
                yield return ClearBuilder();
                int start = i;
                i++;
                for (; i < text.Length && text[i] == ' '; i++);
                i--;
                yield return "\n";
                yield return $"{(i - start) / 4}";
                continue;
            }

            bool matched = false;
            foreach (string delimiter in Symbols)
            if (Match(text, i, delimiter)) {
                yield return ClearBuilder();
                yield return delimiter;
                i += delimiter.Length - 1;
                matched = true;
                break;
            }

            if (matched) {
                continue;
            }

            if (text[i] == ' ') {
                yield return ClearBuilder();
                continue;
            }

            builder.Append(text[i]);
        }

        yield return "\n";

        string ClearBuilder() {
            string built = builder.ToString();
            builder.Clear();
            return built;
        }
    }

    private static bool Match(string text, int index, string match) {
        return string.Compare(text, index, match, 0, match.Length) == 0;
    }
}
