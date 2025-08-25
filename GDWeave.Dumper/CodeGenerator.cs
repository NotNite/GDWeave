using GDWeave.Godot;
using GDWeave.Godot.Variants;

public class CodeGenerator(List<Token> tokens, List<string> identifiers) {
    public void Generate(StreamWriter writer) {
        var onNewLine = false;
        foreach (var token in tokens) {
            var tabs = 0u;
            var gen = this.GenerateToken(token, ref tabs);

            onNewLine = gen == "\n";

            if (!onNewLine)
            {
                gen += ' ';
            }

            writer.Write(gen);

            for (var i = 0; i < tabs; i++) {
                writer.Write('\t');
            }
        }
    }

    private string GenerateToken(Token token, ref uint tabs) {
        var data = (int?) token.AssociatedData ?? 0;
        switch (token.Type) {
            case TokenType.Empty:
                break;
            case TokenType.Identifier:
                return $"{identifiers[data]}";
            case TokenType.Constant:
                var constantToken = (ConstantToken)token;
                if (
                    constantToken.Value is StringVariant stringVariant
                    && stringVariant.GetValue() is string str
                ) { return $"\"{str}\""; }
                return constantToken.Value.GetValue().ToString() ?? "<Failed to convert to string>";
            case TokenType.Newline:
                tabs = token.AssociatedData ?? 0;
                return "\n";
            case TokenType.Self:
                return "self";
            case TokenType.BuiltInType:
                return Enum.GetName((VariantType) data) ?? "<Invalid builtin type>";
            case TokenType.BuiltInFunc:
                return Enum.GetName((BuiltinFunction) data) ?? "<Invalid builtin function>";
            case TokenType.OpIn:
                return "in";
            case TokenType.OpEqual:
                return "==";
            case TokenType.OpNotEqual:
                return "!=";
            case TokenType.OpLess:
                return "<";
            case TokenType.OpLessEqual:
                return "<=";
            case TokenType.OpGreater:
                return ">=";
            case TokenType.OpGreaterEqual:
                return ">=";
            case TokenType.OpAnd:
                return "&&";
            case TokenType.OpOr:
                return "||";
            case TokenType.OpNot:
                return "!";

            case TokenType.OpAdd:
                return "+";
            case TokenType.OpSub:
                return "-";
            case TokenType.OpMul:
                return "*";
            case TokenType.OpDiv:
                return "/";
            case TokenType.OpShiftLeft:
                return "<<";
            case TokenType.OpShiftRight:
                return ">>";
            case TokenType.OpAssign:
                return "=";

            case TokenType.OpAssignAdd:
                return "+=";
            case TokenType.OpAssignSub:
                return "-=";
            case TokenType.OpAssignMul:
                return "*=";
            case TokenType.OpAssignDiv:
                return "/=";
            case TokenType.OpAssignShiftLeft:
                return "<<=";
            case TokenType.OpAssignShiftRight:
                return ">>=";

            case TokenType.OpAssignBitAnd:
                return "&=";
            case TokenType.OpAssignBitOr:
                return "|=";
            case TokenType.OpAssignBitXor:
                return "^=";

            case TokenType.OpBitAnd:
                return "&";
            case TokenType.OpBitOr:
                return "|";
            case TokenType.OpBitXor:
                return "^";

            case TokenType.CfIf:
                return "if";
            case TokenType.CfElse:
                return "else";
            case TokenType.CfElif:
                return "elif";
            case TokenType.CfFor:
                return "for";
            case TokenType.CfWhile:
                return "while";
            case TokenType.CfBreak:
                return "break";
            case TokenType.CfContinue:
                return "continue";
            case TokenType.CfPass:
                return "pass";
            case TokenType.CfReturn:
                return "return";
            case TokenType.CfMatch:
                return "match";

            case TokenType.PrFunction:
                return "func";
            case TokenType.PrClass:
                return "class";
            case TokenType.PrClassName:
                return $"{identifiers[data]}";

            case TokenType.PrExtends:
                return "extends";
            case TokenType.PrIs:
                return "is";

            case TokenType.PrOnready:
                return "@onready";
            case TokenType.PrTool:
                return "@tool";
            case TokenType.PrStatic:
                return "static";
            case TokenType.PrExport:
                return "@export";
            // I believe this is incorrect...
            // Can't find a good explanation for this token online
            case TokenType.PrSetget:
                return "setget";
            case TokenType.PrConst:
                return "const";
            case TokenType.PrVar:
                return "var";
            case TokenType.PrAs:
                return "as";
            case TokenType.PrVoid:
                return "void";
            case TokenType.PrEnum:
                return "enum";
            case TokenType.PrPreload:
                return "preload";
            case TokenType.PrAssert:
                return "assert";
            case TokenType.PrYield:
                return "yield";
            case TokenType.PrSignal:
                return "signal";
            case TokenType.PrBreakpoint:
                return "breakpoint";

            case TokenType.PrRemote:
                return "remote";
            // Also not sure if this is right
            case TokenType.PrSync:
                return "sync";
            case TokenType.PrMaster:
                return "master";
            // Believe these are the same
            // Slave likely kept for compatability
            case TokenType.PrSlave:
            case TokenType.PrPuppet:
                return "puppet";

            case TokenType.PrRemotesync:
                return "remotesync";
            case TokenType.PrMastersync:
                return "mastersync";
            case TokenType.PrPuppetsync:
                return "puppetsync";

            case TokenType.BracketOpen:
                return "[";
            case TokenType.BracketClose:
                return "]";

            case TokenType.CurlyBracketOpen:
                return "{";
            case TokenType.CurlyBracketClose:
                return "}";

            case TokenType.ParenthesisOpen:
                return "(";
            case TokenType.ParenthesisClose:
                return ")";

            case TokenType.Comma:
                return ",";
            case TokenType.Semicolon:
                return ";";
            case TokenType.Period:
                return ".";
            case TokenType.QuestionMark:
                return "?";
            case TokenType.Colon:
                return ":";
            case TokenType.Dollar:
                return "$";
            case TokenType.ForwardArrow:
                return "->";
            case TokenType.ConstPi:
                return "PI";
            case TokenType.ConstTau:
                return "TAU";
            case TokenType.Wildcard:
                return "*";
            case TokenType.ConstInf:
                return "INF";
            case TokenType.ConstNan:
                return "NAN";
            // Not sure this is correct
            case TokenType.Error:
                return "Error";
            // Unneeded but keeping for the sake of it :D
            case TokenType.Eof:
                return "";
            // Not sure what this is for either
            case TokenType.Cursor:
                return "cursor";
            case TokenType.OpMod:
                return "%";
            case TokenType.OpAssignMod:
                return "%=";
            case TokenType.OpBitInvert:
                return "~";
        }

        throw new ArgumentOutOfRangeException();
    }
}
