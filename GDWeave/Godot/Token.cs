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

