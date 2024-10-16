namespace GDWeave.Parser;

public class TokenParser {
    public const int TokenByteMask = 0x80;
    public const int TokenBits = 8;
    public const int TokenMask = (1 << TokenBits) - 1;

    public static Token Read(BinaryReader br) {
        var b = br.ReadByte();
        if ((b & TokenByteMask) != 0) {
            br.BaseStream.Seek(-1, SeekOrigin.Current);
            var token = (uint) (br.ReadUInt32() & ~TokenByteMask);

            var type = (TokenType) (token & TokenMask);
            var associatedData = token >> TokenBits;

            return new Token(type, associatedData);
        } else {
            return new Token((TokenType) b);
        }
    }

    public static void Write(BinaryWriter bw, Token token) {
        if (token.AssociatedData.HasValue) {
            var encodedToken = (uint) token.Type | (token.AssociatedData.Value << TokenBits);
            var buf4 = BitConverter.GetBytes(encodedToken | TokenByteMask);
            for (var j = 0; j < 4; j++) bw.Write(buf4[j]);
        } else {
            bw.Write((byte) token.Type);
        }
    }
}
