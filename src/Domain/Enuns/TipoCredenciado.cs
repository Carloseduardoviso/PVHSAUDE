using System.ComponentModel.DataAnnotations;
namespace PVHSAUDE.Domain.Enuns
{
    public enum TipoCredenciado
    {
        [Display(Name = "Clínica Terapêutica")] Clinica = 1,
        [Display(Name = "Clínica Médica e Especialidade")] Laboratorio = 2,
        [Display(Name = "Laboratório e Exames")] Hospital = 3,
        [Display(Name = "Exames e Imagens")] CentroDiagnostico = 4,
        [Display(Name = "Odontologia")] Farmacia = 5,
        [Display(Name = "Clínicas Populares")] Otica = 6,
        [Display(Name = "Outro menu")] Outro = 7
    }
}
