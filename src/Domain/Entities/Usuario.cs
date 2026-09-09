using PVHSAUDE.Domain.Enuns;
namespace PVHSAUDE.Domain.Entities;

public class Usuario
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string NomeCompleto { get; set; } = "";
    public string Email { get; set; } = "";
    public string EmailNormalizado { get; set; } = "";
    public string SenhaHash { get; set; } = "";
    public Role Role { get; set; }
    public bool Ativo { get; set; } = true;
}
