namespace GDWeave.Godot.Variants;

public class RealVariant : Variant {
    public double Value;
    public bool Is64;

    public RealVariant(VariantParser.ParserReaderContext ctx) {
        this.Value = ctx.Is64 ? ctx.Reader.ReadDouble() : ctx.Reader.ReadSingle();
        this.Is64 = ctx.Is64;
    }

    public RealVariant(double value, bool is64 = false) {
        this.Value = value;
        this.Is64 = is64;
    }

    public override void Write(VariantParser.ParserWriterContext ctx) {
        ctx.WriteType(VariantType.Real, is64: this.Is64);
        if (this.Is64) {
            ctx.Writer.Write(this.Value);
        } else {
            ctx.Writer.Write((float) this.Value);
        }
    }

    public override bool Equals(Variant? other) {
        // Comparing by floats is fine here since no arithmetic is done unless the user modifies it
        // (in which case it's like 99% a different float)
        // Even if we do mess this up, the only penalty is a slight increase in constant counts
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        return other is RealVariant variant && this.Value == variant.Value;
    }

    public override string ToString() {
        return $"RealVariant({this.Value})";
    }

    public override object Clone() {
        return new RealVariant(this.Value, this.Is64);
    }

    public override object GetValue() {
        return Value;
    }
}
