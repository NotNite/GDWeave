namespace GDWeave.Godot.Variants;

// Why does this exist
public class NilVariant : Variant {
    public NilVariant(VariantParser.ParserReaderContext ctx) { }
    private NilVariant() { }

    public override void Write(VariantParser.ParserWriterContext ctx) {
        ctx.WriteType(VariantType.Nil);
    }

    public override bool Equals(Variant? other) {
        return other is NilVariant;
    }

    public override string ToString() {
        return "NilVariant";
    }

    public override object Clone() {
        return new NilVariant();
    }

    public override object GetValue() {
        return "nil";
    }
}
