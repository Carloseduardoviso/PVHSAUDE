namespace PVHSAUDE.Api.Services;

public class EmailOptions
{
    public const string Section = "Email";
    public bool Enabled { get; set; }
    public string EmailHost { get; set; } = "smtp.gmail.com";
    public int EmailPorta { get; set; } = 587;
    public string EmailRemetente { get; set; } = "pvhsaude@pvhsaude.com.br";
    public string EmailSenha { get; set; } = string.Empty;
    public int[] DiasAntesValidade { get; set; } = [7, 1, 0];
}
