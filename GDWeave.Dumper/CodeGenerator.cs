using GDWeave.Godot;
using GDWeave.Godot.Variants;

public class CodeGenerator(List<Token> tokens, List<string> identifiers) {
    Dictionary<int, string> builtinFunctionLiteral = new() {
        [(int) BuiltinFunction.MathSin] = "sin",
        [(int) BuiltinFunction.MathCos] = "cos",
        [(int) BuiltinFunction.MathTan] = "tan",
        [(int) BuiltinFunction.MathSinh] = "sinh",
        [(int) BuiltinFunction.MathCosh] = "cosh",
        [(int) BuiltinFunction.MathTanh] = "tanh",
        [(int) BuiltinFunction.MathAsin] = "asin",
        [(int) BuiltinFunction.MathAcos] = "acos",
        [(int) BuiltinFunction.MathAtan] = "atan",
        [(int) BuiltinFunction.MathAtan2] = "atan2",
        [(int) BuiltinFunction.MathSqrt] = "sqrt",
        [(int) BuiltinFunction.MathFmod] = "fmod",
        [(int) BuiltinFunction.MathFposmod] = "fposmod",
        [(int) BuiltinFunction.MathPosmod] = "posmod",
        [(int) BuiltinFunction.MathFloor] = "floor",
        [(int) BuiltinFunction.MathCeil] = "ceil",
        [(int) BuiltinFunction.MathRound] = "round",
        [(int) BuiltinFunction.MathAbs] = "abs",
        [(int) BuiltinFunction.MathSign] = "sign",
        [(int) BuiltinFunction.MathPow] = "pow",
        [(int) BuiltinFunction.MathLog] = "log",
        [(int) BuiltinFunction.MathExp] = "exp",
        [(int) BuiltinFunction.MathIsnan] = "is_nan",
        [(int) BuiltinFunction.MathIsinf] = "is_inf",
        [(int) BuiltinFunction.MathIsequalapprox] = "is_equal_approx",
        [(int) BuiltinFunction.MathIszeroapprox] = "is_zero_approx",
        [(int) BuiltinFunction.MathEase] = "ease",
        [(int) BuiltinFunction.MathDecimals] = "decimals",
        [(int) BuiltinFunction.MathStepDecimals] = "step_decimals",
        [(int) BuiltinFunction.MathStepify] = "stepify",
        [(int) BuiltinFunction.MathLerp] = "lerp",
        [(int) BuiltinFunction.MathLerpAngle] = "lerp_angle",
        [(int) BuiltinFunction.MathInverseLerp] = "inverse_lerp",
        [(int) BuiltinFunction.MathRangeLerp] = "range_lerp",
        [(int) BuiltinFunction.MathSmoothstep] = "smoothstep",
        [(int) BuiltinFunction.MathMoveToward] = "move_toward",
        [(int) BuiltinFunction.MathDectime] = "dectime",
        [(int) BuiltinFunction.MathRandomize] = "randomize",
        [(int) BuiltinFunction.MathRand] = "randi",
        [(int) BuiltinFunction.MathRandf] = "randf",
        [(int) BuiltinFunction.MathRandom] = "rand_range",
        [(int) BuiltinFunction.MathSeed] = "seed",
        [(int) BuiltinFunction.MathSeed] = "rand_seed",
        [(int) BuiltinFunction.MathDeg2Rad] = "deg2rad",
        [(int) BuiltinFunction.MathRad2Deg] = "rad2deg",
        [(int) BuiltinFunction.MathLinear2Db] = "linear2db",
        [(int) BuiltinFunction.MathDb2Linear] = "db2linear",
        [(int) BuiltinFunction.MathPolar2Cartesian] = "polar2cartesian",
        [(int) BuiltinFunction.MathCartesian2Polar] = "cartesian2polar",
        [(int) BuiltinFunction.MathWrap] = "wrapi",
        [(int) BuiltinFunction.MathWrap] = "wrapf",
        [(int) BuiltinFunction.LogicMax] = "max",
        [(int) BuiltinFunction.LogicMin] = "min",
        [(int) BuiltinFunction.LogicClamp] = "clamp",
        [(int) BuiltinFunction.LogicNearestPo2] = "nearest_po2",
        [(int) BuiltinFunction.ObjWeakref] = "weakref",
        [(int) BuiltinFunction.FuncFuncref] = "funcref",
        [(int) BuiltinFunction.TypeConvert] = "convert",
        [(int) BuiltinFunction.TypeOf] = "typeof",
        [(int) BuiltinFunction.TypeExists] = "type_exists",
        [(int) BuiltinFunction.TextChar] = "char",
        [(int) BuiltinFunction.TextOrd] = "ord",
        [(int) BuiltinFunction.TextStr] = "str",
        [(int) BuiltinFunction.TextPrint] = "print",
        [(int) BuiltinFunction.TextPrintTabbed] = "printt",
        [(int) BuiltinFunction.TextPrintSpaced] = "prints",
        [(int) BuiltinFunction.TextPrinterr] = "printerr",
        [(int) BuiltinFunction.TextPrintraw] = "printraw",
        [(int) BuiltinFunction.TextPrintDebug] = "print_debug",
        [(int) BuiltinFunction.PushError] = "push_error",
        [(int) BuiltinFunction.PushWarning] = "push_warning",
        [(int) BuiltinFunction.VarToStr] = "var2str",
        [(int) BuiltinFunction.StrToVar] = "str2var",
        [(int) BuiltinFunction.VarToBytes] = "var2bytes",
        [(int) BuiltinFunction.BytesToVar] = "bytes2var",
        [(int) BuiltinFunction.GenRange] = "range",
        [(int) BuiltinFunction.ResourceLoad] = "load",
        [(int) BuiltinFunction.Inst2Dict] = "inst2dict",
        [(int) BuiltinFunction.Dict2Inst] = "dict2inst",
        [(int) BuiltinFunction.ValidateJson] = "validate_json",
        [(int) BuiltinFunction.ParseJson] = "parse_json",
        [(int) BuiltinFunction.ToJson] = "to_json",
        [(int) BuiltinFunction.Hash] = "hash",
        [(int) BuiltinFunction.Color8] = "Color8",
        [(int) BuiltinFunction.Colorn] = "ColorN",
        [(int) BuiltinFunction.PrintStack] = "print_stack",
        [(int) BuiltinFunction.GetStack] = "get_stack",
        [(int) BuiltinFunction.InstanceFromId] = "instance_from_id",
        [(int) BuiltinFunction.Len] = "len",
        [(int) BuiltinFunction.IsInstanceValid] = "is_instance_valid",
        [(int) BuiltinFunction.DeepEqual] = "deep_equal",
        [(int) BuiltinFunction.FuncMax] = "MAX",
    };

    public void Generate(StreamWriter writer) {
        var whitespaceExclusions = new List<TokenType>(
            [
                // Identifiers //
                TokenType.BuiltInFunc,
                TokenType.PrPreload,
                TokenType.BuiltInType,
                TokenType.Identifier,
                TokenType.Constant,
                TokenType.ConstInf,
                TokenType.ConstNan,
                TokenType.ConstPi,
                // Brackets, accessors, etc //
                TokenType.BracketOpen,
                TokenType.BracketClose,
                TokenType.ParenthesisOpen,
                TokenType.ParenthesisClose,
                TokenType.Period,
                TokenType.Dollar,
                // Operators, Keywords //
                TokenType.OpDiv,
                TokenType.OpSub,
                TokenType.PrYield,
                TokenType.CfElse,
            ]
        );

        var binaryOperators = new List<TokenType>(
            [
                TokenType.OpIn,
                TokenType.OpAnd,
                TokenType.OpOr,
                TokenType.OpEqual,
                TokenType.OpGreater,
                TokenType.OpGreaterEqual,
                TokenType.OpLess,
                TokenType.OpLessEqual,
                TokenType.OpNotEqual,
                TokenType.OpAssign,
                TokenType.OpAdd,
                // TokenType.OpSub also used for negative numbers
                TokenType.OpMul,
                // TokenType.OpDiv also used for get_node paths
                TokenType.OpMod,
                TokenType.OpAssignAdd,
                TokenType.OpAssignSub,
                TokenType.OpAssignDiv,
                TokenType.OpAssignMul,
                TokenType.OpAssignMod,
            ]
        );

        foreach (var token in tokens) {
            var tabs = 0u;
            var gen = this.GenerateToken(token, ref tabs);
            var onNewLine = gen == "\n";

            if (binaryOperators.Contains(token.Type)) {
                gen = $" {gen} ";
            } else if (!onNewLine && !whitespaceExclusions.Contains(token.Type)) {
                gen += ' ';
            }

            writer.Write(gen);

            for (var i = 0; i < tabs; i++) {
                writer.Write('\t');
            }

            // If & Else keywords toggle for handling if-else and ternary cases
            if (token.Type == TokenType.Newline) {
                binaryOperators.Remove(TokenType.CfIf);
                binaryOperators.Remove(TokenType.CfElse);
            } else {
                if (!binaryOperators.Contains(TokenType.CfIf)) {
                    binaryOperators.Add(TokenType.CfIf);
                    binaryOperators.Add(TokenType.CfElse);
                }
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
                var constantToken = (ConstantToken) token;
                if (
                    constantToken.Value is StringVariant stringVariant
                    && stringVariant.GetValue() is string str
                ) {
                    // Prevent output of processed escape sequences
                    // (These are literal chars so it is insufficient to escape backslashes as a catch-all)
                    str = str.Replace("\n", "\\n");
                    str = str.Replace("\t", "\\t");
                    return '"' + str + '"';
                } else if (constantToken.Value is BoolVariant boolVariant) {
                    // Lowercase to match GDScript
                    return (bool) boolVariant.GetValue() ? "true" : "false";
                } else if (constantToken.Value is RealVariant realVariant) {
                    // Refrain from omitting trailing zeros
                    var value = (double) realVariant.GetValue();
                    return value.ToString("F");
                } else {
                    return constantToken.Value.GetValue().ToString()
                        ?? "<Failed to convert to string>";
                }
            case TokenType.Newline:
                tabs = token.AssociatedData ?? 0;
                return "\n";
            case TokenType.Self:
                return "self";
            case TokenType.BuiltInType:
                return Enum.GetName((VariantType) data) ?? "<Invalid builtin type>";
            case TokenType.BuiltInFunc:
                return builtinFunctionLiteral.ContainsKey(data)
                    ? builtinFunctionLiteral[data]
                    : "<Invalid builtin function>";
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
                return "and";
            case TokenType.OpOr:
                return "or";
            case TokenType.OpNot:
                return "not";

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
                return "onready";
            case TokenType.PrTool:
                return "@tool";
            case TokenType.PrStatic:
                return "static";
            case TokenType.PrExport:
                return "export";
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
