using System.ComponentModel.DataAnnotations;
using PVHSAUDE.Domain.Entities;
namespace PVHSAUDE.Application.ViewModels;
public record CatalogoEntradaVm([Required]string Nome, bool Ativo = true);
public record EspecialidadeRespostaVm(Guid Id,string Nome,bool Ativo);
public record ProcedimentoRespostaVm(Guid Id,string Nome,bool Ativo);
public record DescontoRespostaVm(Guid Id, string Nome, bool Ativo);
