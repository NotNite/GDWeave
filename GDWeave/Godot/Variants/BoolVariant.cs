namespace GDWeave.Godot.Variants;

public class BoolVariant : Variant {
    public bool Value;

    public BoolVariant(VariantParser.ParserReaderContext ctx) {
        // WHY
        this.Value = ctx.Reader.ReadUInt32() != 0;
    }

    public BoolVariant(bool value) {
        this.Value = value;
    }

    public override void Write(VariantParser.ParserWriterContext ctx) {
        ctx.WriteType(VariantType.Bool);
        ctx.Writer.Write(this.Value ? 1u : 0u);
    }

    public override bool Equals(Variant? other) {
        return other is BoolVariant variant && this.Value == variant.Value;
    }

    public override string ToString() {
        return $"BoolVariant({this.Value})";
    }

    public override object Clone() {
        return new BoolVariant(this.Value);
    }
}
