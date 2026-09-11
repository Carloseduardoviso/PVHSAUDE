using System.ComponentModel.DataAnnotations; 
namespace Web.Models;

public class ContatoVm 
{
    public Guid Id{get;set;} 

    [Required(ErrorMessage="Informe o nome.")]
    [Display(Name="Nome")]
    public string Nome{get;set;}="";

    public string Sobrenome{get;set;}=""; 

    [Required,EmailAddress,Display(Name="E-mail")] 
    public string Email{get;set;}="";
    
    [Required,Display(Name="Telefone")] 
    public string Telefone{get;set;}=""; 

    [Required,Display(Name="CPF")] 
    public string Cpf{get;set;}=""; 

    [Required]
    [Display(Name = "Motivo do contato")]
    public string Motivo{get;set;}=""; 

    [Required,MaxLength(4000)]
    [Display(Name = "Mensagem")]
    public string Mensagem{get;set;}="";
    
    public DateTime EnviadoEm{get;set;} }

