using Infra.Data.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PVHSAUDE.Api.Contracts;
using PVHSAUDE.Domain.Entities;

namespace PVHSAUDE.Api.Controllers;

[ApiController]
[Route("api/beneficiarios")]
[AllowAnonymous]
public class BeneficiariosController(Context context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BeneficiarioResponse>>> Listar(CancellationToken cancellationToken)
    {
        var beneficiarios = await context.Beneficiarios.AsNoTracking()
            .Include(x => x.Dependentes)
            .OrderBy(x => x.Nome)
            .ToListAsync(cancellationToken);

        return Ok(beneficiarios.Select(ParaResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BeneficiarioResponse>> Obter(int id, CancellationToken cancellationToken)
    {
        var beneficiario = await context.Beneficiarios.AsNoTracking().Include(x => x.Dependentes)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return beneficiario is null ? NotFound() : Ok(ParaResponse(beneficiario));
    }

    [HttpPost]
    public async Task<ActionResult<BeneficiarioResponse>> Criar(BeneficiarioRequest request, CancellationToken cancellationToken)
    {
        if (request.DataValidade < request.DataInicio)
            return ValidationProblem("A validade do benefício deve ser posterior à data de início.");

        var cpf = NormalizarCpf(request.Cpf);
        if (await context.Beneficiarios.AnyAsync(x => x.Cpf == cpf, cancellationToken))
            return Conflict("Já existe um beneficiário com este CPF.");

        var beneficiario = CriarEntidade(request, cpf);
        context.Beneficiarios.Add(beneficiario);
        await context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Obter), new { beneficiario.Id }, ParaResponse(beneficiario));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, BeneficiarioRequest request, CancellationToken cancellationToken)
    {
        if (request.DataValidade < request.DataInicio)
            return ValidationProblem("A validade do benefício deve ser posterior à data de início.");

        var beneficiario = await context.Beneficiarios.FindAsync([id], cancellationToken);
        if (beneficiario is null) return NotFound();

        var cpf = NormalizarCpf(request.Cpf);
        if (await context.Beneficiarios.AnyAsync(x => x.Id != id && x.Cpf == cpf, cancellationToken))
            return Conflict("Já existe um beneficiário com este CPF.");

        Aplicar(beneficiario, request, cpf);
        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id, CancellationToken cancellationToken)
    {
        var beneficiario = await context.Beneficiarios.FindAsync([id], cancellationToken);
        if (beneficiario is null) return NotFound();

        context.Beneficiarios.Remove(beneficiario);
        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static Beneficiario CriarEntidade(BeneficiarioRequest request, string cpf)
    {
        var beneficiario = new Beneficiario(request.Nome.Trim(), cpf, request.DataNascimento, request.PlanoId, request.DataInicio, request.DataValidade);
        Aplicar(beneficiario, request, cpf);
        return beneficiario;
    }

    private static void Aplicar(Beneficiario beneficiario, BeneficiarioRequest request, string cpf) =>
        beneficiario.Atualizar(request.Nome.Trim(), cpf, request.DataNascimento, request.Telefone?.Trim(), request.Email?.Trim(), request.Endereco?.Trim(), request.PlanoId, request.DataInicio, request.DataValidade, request.Status);

    private static string NormalizarCpf(string cpf) => new(cpf.Where(char.IsDigit).ToArray());

    private static BeneficiarioResponse ParaResponse(Beneficiario x) => new(x.Id, x.Nome, x.Cpf, x.DataNascimento, x.Telefone, x.Email, x.Endereco, x.PlanoId, x.DataInicio, x.DataValidade, x.Status,
        x.Dependentes.Select(d => new DependenteResponse(d.Id, d.Nome, d.Cpf, d.DataNascimento, d.GrauParentesco)).ToList());
}
