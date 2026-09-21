using System.ComponentModel.DataAnnotations;
using PVHSAUDE.Domain.Enuns;
namespace PVHSAUDE.Application.ViewModels;
public class BannerVm
{
    public Guid Id { get; set; }
    [Required(ErrorMessage = "Informe o título."), StringLength(200), Display(Name = "Título")]
    public string Titulo { get; set; } = "";
    public PosicaoBanner Posicao { get; set; } = PosicaoBanner.Central;
    public bool Ativo { get; set; } = true;
}