namespace PVHSAUDE.Application.AppService;

public static class LogoPortalFormato
{
    public const string Mensagem = "A logo deve estar na proporção 6:1 (recomendado: 1920 × 320 pixels).";

    public static bool Valido(byte[] bytes)
    {
        var (w, h) = BannerFormato.Dimensoes(bytes);
        return w > 0 && h > 0 && ((long)w == (long)h * 6 || Math.Abs((double)w / h - 6.0) <= 0.02);
    }
}
