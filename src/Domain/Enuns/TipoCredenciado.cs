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
        [Display(Name = "Clínicas Populares")] ClinicasPopulares = 6,
        [Display(Name = "Academia")] Academia = 7,
        [Display(Name = "Futebol")] Futebol = 8,
        [Display(Name = "Natação")] Natacao = 9,
        [Display(Name = "Ótica")] Otica = 10,
        [Display(Name = "Roupa Esportiva")] RoupaEsportiva = 11
    }
}
