namespace GDWeave.Parser;

public abstract class Variant : IEquatable<Variant>, ICloneable {
    public abstract void Write(VariantParser.ParserWriterContext ctx);
    public abstract bool Equals(Variant? other);
    public abstract object Clone();
}

public enum VariantType : uint {
    Nil,

    // atomic types
    Bool,
    Int,
    Real,
    String,

    // math types
    Vector2,
    Rect2,
    Vector3,
    Transform2D,
    Plane,
    Quat,
    AaBb,
    Basis,
    Transform,

    // misc types
    Color,
    NodePath,
    Rid,
    Object,
    Dictionary,
    Array,

    // arrays
    PoolByteArray,
    PoolIntArray,
    PoolRealArray,
    PoolStringArray,
    PoolVector2Array,
    PoolVector3Array,
    PoolColorArray
}
