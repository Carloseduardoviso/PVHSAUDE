using System.ComponentModel.DataAnnotations;

namespace PVHSAUDE.Domain.Enuns;

public enum GrauParentesco
{
    [Display(Name = "Cônjuge")] Conjuge = 1,
    [Display(Name = "Companheiro(a)")] Companheiro = 2,
    [Display(Name = "Filho(a)")] Filho = 3,
    [Display(Name = "Enteado(a)")] Enteado = 4,
    [Display(Name = "Pai")] Pai = 5,
    [Display(Name = "Mãe")] Mae = 6,
    [Display(Name = "Irmão(ã)")] Irmao = 7,
    [Display(Name = "Avó")] Avo = 8,
    [Display(Name = "Avô")] AvoMasculino = 9,
    [Display(Name = "Neto(a)")] Neto = 10,
    [Display(Name = "Tio(a)")] Tio = 11,
    [Display(Name = "Sobrinho(a)")] Sobrinho = 12,
    [Display(Name = "Primo(a)")] Primo = 13,
    [Display(Name = "Sogro(a)")] Sogro = 14,
    [Display(Name = "Genro")] Genro = 15,
    [Display(Name = "Nora")] Nora = 16,
    [Display(Name = "Cunhado(a)")] Cunhado = 17,
    [Display(Name = "Tutelado(a)")] Tutelado = 18,
    [Display(Name = "Outros")] Outros = 19
}
