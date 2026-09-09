using System.Buffers.Binary;
namespace PVHSAUDE.Api.Configs;

public static class BannerFormato
{
    public const string Mensagem = "Use uma imagem em paisagem na proporção 16:5, por exemplo 1600 × 500 pixels (JPG, PNG ou WEBP).";
    public static bool Valido(byte[] bytes)
    {
        var (w, h) = Dimensoes(bytes);
        return w > 0 && h > 0 && (long)w * 5 == (long)h * 16;
    }
    public static (int Width, int Height) Dimensoes(byte[] bytes)
    {
        var b = bytes.AsSpan();
        if (b.Length >= 24 && b[..8].SequenceEqual(new byte[] {137,80,78,71,13,10,26,10}) && b.Slice(12,4).SequenceEqual("IHDR"u8))
            return (BinaryPrimitives.ReadInt32BigEndian(b[16..]), BinaryPrimitives.ReadInt32BigEndian(b[20..]));
        if (b.Length >= 12 && b[..4].SequenceEqual("RIFF"u8) && b.Slice(8,4).SequenceEqual("WEBP"u8))
        {
            if (b.Length >= 30 && b.Slice(12,4).SequenceEqual("VP8X"u8))
                return (1 + b[24] + (b[25] << 8) + (b[26] << 16), 1 + b[27] + (b[28] << 8) + (b[29] << 16));
            if (b.Length >= 30 && b.Slice(12,4).SequenceEqual("VP8 "u8) && b.Slice(23,3).SequenceEqual(new byte[] {157,1,42}))
                return (BinaryPrimitives.ReadUInt16LittleEndian(b[26..]) & 16383, BinaryPrimitives.ReadUInt16LittleEndian(b[28..]) & 16383);
            if (b.Length >= 25 && b.Slice(12,4).SequenceEqual("VP8L"u8) && b[20] == 47)
                return (1 + b[21] + ((b[22] & 63) << 8), 1 + (b[22] >> 6) + (b[23] << 2) + ((b[24] & 15) << 10));
        }
        if (b.Length >= 4 && b[0] == 255 && b[1] == 216)
        {
            var i = 2;
            while (i < b.Length)
            {
                if (b[i++] != 255) return default;
                while (i < b.Length && b[i] == 255) i++;
                if (i >= b.Length) break;
                var marker = b[i++];
                if (marker is 0xDA or 0xD9) break;
                if (marker is 0x01 or >= 0xD0 and <= 0xD7) continue;
                if (i + 2 > b.Length) break;
                var length = BinaryPrimitives.ReadUInt16BigEndian(b[i..]);
                if (length < 2 || i + length > b.Length) break;
                if (marker is 0xC0 or 0xC1 or 0xC2 or 0xC3 or 0xC5 or 0xC6 or 0xC7 or 0xC9 or 0xCA or 0xCB or 0xCD or 0xCE or 0xCF)
                    return length >= 8 ? (BinaryPrimitives.ReadUInt16BigEndian(b[(i + 5)..]), BinaryPrimitives.ReadUInt16BigEndian(b[(i + 3)..])) : default;
                i += length;
            }
        }
        return default;
    }
}
