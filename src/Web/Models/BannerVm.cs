using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PVHSAUDE.Domain.Enuns;
namespace Web.Models;
public class BannerVm
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [Required(ErrorMessage = "Informe o título."), StringLength(200), Display(Name = "Título")]
    public string Titulo { get; set; } = "";
    public PosicaoBanner Posicao { get; set; } = PosicaoBanner.Central;
    public bool Ativo { get; set; } = true;
    public IFormFile? Imagem { get; set; }
}