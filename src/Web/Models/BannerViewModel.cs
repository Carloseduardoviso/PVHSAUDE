using System.ComponentModel.DataAnnotations;
namespace Web.Models;
public class BannerViewModel
{
    public Guid Id { get; set; }
    [Required(ErrorMessage = "Informe o título."), StringLength(200), Display(Name = "Título")]
    public string Titulo { get; set; } = "";
    public bool Ativo { get; set; } = true;
    public IFormFile? Imagem { get; set; }
}
