using PVHSAUDE.Application.AppService;
using PVHSAUDE.Domain.Enuns;

internal static class BannerFormatTests
{
    public static void Run()
    {
        var lateral = Png(400, 500);
        var central = Png(1600, 500);
        Check(BannerFormato.Valido(lateral), "lateral aceita imagem em retrato 4:5");
        Check(BannerFormato.Valido(lateral, PosicaoBanner.LateralDireita), "lateral direita exige e aceita 4:5");
        Check(!BannerFormato.Valido(lateral, PosicaoBanner.Central), "central rejeita imagem de lateral");
        Check(BannerFormato.Valido(central, PosicaoBanner.InferiorEsquerda), "inferior esquerdo aceita paisagem 16:5");
    }

    private static byte[] Png(int width, int height)
    {
        var bytes = new byte[24];
        bytes[0] = 137; bytes[1] = 80; bytes[2] = 78; bytes[3] = 71;
        bytes[4] = 13; bytes[5] = 10; bytes[6] = 26; bytes[7] = 10;
        bytes[12] = (byte)'I'; bytes[13] = (byte)'H'; bytes[14] = (byte)'D'; bytes[15] = (byte)'R';
        System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(bytes.AsSpan(16), width);
        System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(bytes.AsSpan(20), height);
        return bytes;
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception("Banner: " + message);
        Console.WriteLine("PASS: Banner - " + message);
    }
}