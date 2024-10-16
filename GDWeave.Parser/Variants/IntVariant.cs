namespace GDWeave.Parser.Variants;

public class IntVariant : Variant {
    public long Value;
    public bool Is64;

    public IntVariant(VariantParser.ParserReaderContext ctx) {
        this.Value = ctx.Is64 ? ctx.Reader.ReadInt64() : ctx.Reader.ReadInt32();
        this.Is64 = ctx.Is64;
    }

    public IntVariant(long value, bool is64 = false) {
        this.Value = value;
        this.Is64 = is64;
    }

    public override void Write(VariantParser.ParserWriterContext ctx) {
        ctx.WriteType(VariantType.Int, is64: this.Is64);
        if (this.Is64) {
            ctx.Writer.Write(this.Value);
        } else {
            ctx.Writer.Write((int) this.Value);
        }
    }

    public override bool Equals(Variant? other) {
        if (other is not IntVariant) return false;
        return other is IntVariant variant && this.Value == variant.Value;
    }

    public override string ToString() {
        return $"IntVariant({this.Value})";
    }

    public override object Clone() {
        return new IntVariant(this.Value, this.Is64);
    }
}
