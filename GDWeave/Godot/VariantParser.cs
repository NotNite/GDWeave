// TODO: engine-dependent

using GDWeave.Godot.Variants;

namespace GDWeave.Godot;

public class VariantParser {
    public const byte EncodeMask = 0xFF;
    public const int EncodeFlag64 = 1 << 16;

    public static Dictionary<VariantType, Func<ParserReaderContext, Variant>> Parsers = new() {
        {VariantType.Nil, ctx => new NilVariant(ctx)},
        {VariantType.Bool, ctx => new BoolVariant(ctx)},
        {VariantType.Int, ctx => new IntVariant(ctx)},
        {VariantType.Real, ctx => new RealVariant(ctx)},
        {VariantType.String, ctx => new StringVariant(ctx)},
    };

    public static Variant Read(BinaryReader br) {
        var type = br.ReadUInt32();
        var typeEnum = (VariantType) (type & EncodeMask);
        var is64 = (type & EncodeFlag64) != 0;

        var ctx = new ParserReaderContext {
            Reader = br,
            Is64 = is64
        };

        if (Parsers.TryGetValue(typeEnum, out var parser)) {
            var variant = parser(ctx);
            return variant;
        }

        throw new InvalidDataException($"Unknown variant type: {typeEnum}");
    }

    public static void Write(BinaryWriter bw, Variant variant) {
        var ctx = new ParserWriterContext {
            Writer = bw
        };

        Write(ctx, variant);
    }

    public static void Write(ParserWriterContext ctx, Variant variant) {
        variant.Write(ctx);
    }

    public record ParserReaderContext {
        public required BinaryReader Reader;
        public bool Is64;
    }

    public record ParserWriterContext {
        public required BinaryWriter Writer;

        public void WriteType(VariantType type, bool is64 = false) {
            var encodedType = (uint) type;
            if (is64) encodedType |= EncodeFlag64;
            this.Writer.Write(encodedType);
        }
    }
}
