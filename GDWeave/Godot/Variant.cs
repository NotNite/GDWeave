// TODO: engine-dependent

namespace GDWeave.Godot;

public abstract class Variant : IEquatable<Variant>, ICloneable {
    public abstract void Write(VariantParser.ParserWriterContext ctx);
    public abstract bool Equals(Variant? other);
    public abstract object Clone();
    public abstract object GetValue();
}

