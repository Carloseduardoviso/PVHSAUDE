using PVHSAUDE.Application.AppService;

internal static class LogoPortalFormatTests
{
    public static void Run()
    {
        var logo1920x320 = Png(1920, 320);
        var logo600x100 = Png(600, 100);
        var lateral = Png(800, 1000);
        var central = Png(1920, 600);
        var fullHd = Png(1920, 1080);

        Check(LogoPortalFormato.Valido(logo1920x320), "logo aceita 1920x320 na proporção 6:1");
        Check(LogoPortalFormato.Valido(logo600x100), "logo aceita 600x100 na proporção 6:1");
        Check(!LogoPortalFormato.Valido(lateral), "logo rejeita proporção 4:5");
        Check(!LogoPortalFormato.Valido(central), "logo rejeita proporção 16:5");
        Check(!LogoPortalFormato.Valido(fullHd), "logo rejeita proporção 16:9");
        Check(!LogoPortalFormato.Valido(Array.Empty<byte>()), "logo rejeita bytes vazios");
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
        if (!condition) throw new Exception("LogoPortal: " + message);
        Console.WriteLine("PASS: LogoPortal - " + message);
    }
}
