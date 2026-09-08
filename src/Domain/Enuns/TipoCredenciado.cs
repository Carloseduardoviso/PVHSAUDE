using System.ComponentModel.DataAnnotations;
namespace PVHSAUDE.Domain.Enuns
{
    public enum TipoCredenciado
    {
        [Display(Name = "Clínica")] Clinica = 1, 
        [Display(Name = "Laboratório")] Laboratorio = 2, 
        Hospital = 3, 
        [Display(Name = "Centro de diagnóstico")] CentroDiagnostico = 4, 
        [Display(Name = "Farmácia")] Farmacia = 5, 
        [Display(Name = "Ótica")] Otica = 6, 
        Outro = 7
    }
}
