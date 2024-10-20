// TODO: engine-dependent

using System.Diagnostics.CodeAnalysis;

namespace GDWeave.Godot;

public class Token {
    public TokenType Type;
    public uint? AssociatedData;

    public Token(TokenType type, uint? associatedData = null) {
        this.Type = type;
        this.AssociatedData = associatedData;
    }

    public override string ToString() {
        return $"Token({this.Type}, {this.AssociatedData})";
    }
}

// used for high level
public class ConstantToken : Token {
    public required Variant Value;

    [SetsRequiredMembers]
    public ConstantToken(Variant value) : base(TokenType.Constant) {
        this.Value = value;
    }

    [SetsRequiredMembers]
    public ConstantToken(TokenType type, uint? associatedData, Variant value) : base(type, associatedData) {
        this.Value = value;
    }

    public override string ToString() {
        return $"ConstantToken({this.Type}, {this.AssociatedData}, {this.Value})";
    }
}

public class IdentifierToken : Token {
    public required string Name;

    [SetsRequiredMembers]
    public IdentifierToken(string name) : base(TokenType.Identifier) {
        this.Name = name;
    }

    [SetsRequiredMembers]
    public IdentifierToken(TokenType type, uint? associatedData, string name) : base(type, associatedData) {
        this.Name = name;
    }

    public override string ToString() {
        return $"IdentifierToken({this.Type}, {this.AssociatedData}, {this.Name})";
    }
}

public enum TokenType {
    Empty,
    Identifier,
    Constant,
    Self,
    BuiltInType,
    BuiltInFunc,
    OpIn,
    OpEqual,
    OpNotEqual,
    OpLess,
    OpLessEqual,
    OpGreater,
    OpGreaterEqual,
    OpAnd,
    OpOr,
    OpNot,
    OpAdd,
    OpSub,
    OpMul,
    OpDiv,
    OpMod,
    OpShiftLeft,
    OpShiftRight,
    OpAssign,
    OpAssignAdd,
    OpAssignSub,
    OpAssignMul,
    OpAssignDiv,
    OpAssignMod,
    OpAssignShiftLeft,
    OpAssignShiftRight,
    OpAssignBitAnd,
    OpAssignBitOr,
    OpAssignBitXor,
    OpBitAnd,
    OpBitOr,
    OpBitXor,
    OpBitInvert,
    CfIf,
    CfElif,
    CfElse,
    CfFor,
    CfWhile,
    CfBreak,
    CfContinue,
    CfPass,
    CfReturn,
    CfMatch,
    PrFunction,
    PrClass,
    PrClassName,
    PrExtends,
    PrIs,
    PrOnready,
    PrTool,
    PrStatic,
    PrExport,
    PrSetget,
    PrConst,
    PrVar,
    PrAs,
    PrVoid,
    PrEnum,
    PrPreload,
    PrAssert,
    PrYield,
    PrSignal,
    PrBreakpoint,
    PrRemote,
    PrSync,
    PrMaster,
    PrSlave,
    PrPuppet,
    PrRemotesync,
    PrMastersync,
    PrPuppetsync,
    BracketOpen,
    BracketClose,
    CurlyBracketOpen,
    CurlyBracketClose,
    ParenthesisOpen,
    ParenthesisClose,
    Comma,
    Semicolon,
    Period,
    QuestionMark,
    Colon,
    Dollar,
    ForwardArrow,
    Newline,
    ConstPi,
    ConstTau,
    Wildcard,
    ConstInf,
    ConstNan,
    Error,
    Eof,
    Cursor
}
