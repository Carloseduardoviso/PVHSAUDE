using PVHSAUDE.Domain.Enuns;
using System.ComponentModel.DataAnnotations;

namespace PVHSAUDE.Application.ViewModels
{
    public class UsuarioVm
    {
        public string[] Menus { get; set; } = [];
        public Guid UsuarioId { get; set; }

        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Nome Completo")]
        public string? NomeCompleto { get; set; }

        [Display(Name = "Permissão")]
        public Role Role { get; set; }
        public bool Ativo { get; set; } = true;
    }
}
