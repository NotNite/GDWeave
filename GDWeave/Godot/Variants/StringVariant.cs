using System.Text;

namespace GDWeave.Godot.Variants;

public class StringVariant : Variant {
    public string Value;

    public StringVariant(VariantParser.ParserReaderContext ctx) {
        var length = ctx.Reader.ReadInt32();
        var pad = 0;
        if (length % 4 != 0) {
            pad = 4 - (length % 4);
        }

        var bytes = ctx.Reader.ReadBytes(length);
        this.Value = Encoding.UTF8.GetString(bytes);
        ctx.Reader.ReadBytes(pad);
    }

    public StringVariant(string value) {
        this.Value = value;
    }

    public override void Write(VariantParser.ParserWriterContext ctx) {
        ctx.WriteType(VariantType.String);
        var bytes = Encoding.UTF8.GetBytes(this.Value);
        ctx.Writer.Write(bytes.Length);
        ctx.Writer.Write(bytes);
        if (bytes.Length % 4 != 0) {
            var pad = 4 - (bytes.Length % 4);
            ctx.Writer.Write(new byte[pad]);
        }
    }

    public override bool Equals(Variant? other) {
        return other is StringVariant variant && this.Value.Equals(variant.Value);
    }

    public override string ToString() {
        return $"StringVariant({this.Value})";
    }

    public override object Clone() {
        return new StringVariant(this.Value);
    }
}
