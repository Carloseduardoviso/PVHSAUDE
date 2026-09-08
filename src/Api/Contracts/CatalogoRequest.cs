using System.ComponentModel.DataAnnotations;
using PVHSAUDE.Domain.Entities;
namespace PVHSAUDE.Api.Contracts;
public record CatalogoRequest([Required]string Nome, bool Ativo = true);
public record EspecialidadeResponse(Guid Id,string Nome,bool Ativo);
public record ProcedimentoResponse(Guid Id,string Nome,bool Ativo);
