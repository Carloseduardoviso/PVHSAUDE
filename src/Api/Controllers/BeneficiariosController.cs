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

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BeneficiarioResponse>> Obter(Guid id, CancellationToken cancellationToken)
    {
        var beneficiario = await context.Beneficiarios.AsNoTracking().Include(x => x.Dependentes)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return beneficiario is null ? NotFound() : Ok(ParaResponse(beneficiario));
    }

    [HttpPost]
    public async Task<ActionResult<BeneficiarioResponse>> Criar(BeneficiarioRequest request, CancellationToken cancellationToken)
    {
        var planoEmpresa = await context.Credenciados.Where(x => x.Id == request.CredenciadoId).Select(x => x.PlanoId).FirstOrDefaultAsync(cancellationToken);
        if (planoEmpresa is null) return BadRequest("Selecione uma empresa com plano cadastrado.");
        request.PlanoId = planoEmpresa.Value;

        if (request.DataValidade < request.DataInicio)
            return ValidationProblem("A validade do benefício deve ser posterior à data de início.");

        var cpf = NormalizarCpf(request.Cpf);
        if (await context.Beneficiarios.AnyAsync(x => x.Cpf == cpf, cancellationToken))
            return Conflict("Já existe um beneficiário com este CPF/CNPJ.");

        var beneficiario = CriarEntidade(request, cpf);
        if (request.Dependentes?.Any(x => x.Id != Guid.Empty) == true)
            return BadRequest("Novos dependentes não devem possuir um identificador.");
        SincronizarDependentes(beneficiario, request.Dependentes);
        context.Beneficiarios.Add(beneficiario);
        await context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Obter), new { beneficiario.Id }, ParaResponse(beneficiario));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, BeneficiarioRequest request, CancellationToken cancellationToken)
    {
        var planoEmpresa = await context.Credenciados.Where(x => x.Id == request.CredenciadoId).Select(x => x.PlanoId).FirstOrDefaultAsync(cancellationToken);
        if (planoEmpresa is null) return BadRequest("Selecione uma empresa com plano cadastrado.");
        request.PlanoId = planoEmpresa.Value;

        if (request.DataValidade < request.DataInicio)
            return ValidationProblem("A validade do benefício deve ser posterior à data de início.");

        var beneficiario = await context.Beneficiarios.Include(x => x.Dependentes).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (beneficiario is null) return NotFound();

        var cpf = NormalizarCpf(request.Cpf);
        if (await context.Beneficiarios.AnyAsync(x => x.Id != id && x.Cpf == cpf, cancellationToken))
            return Conflict("Já existe um beneficiário com este CPF/CNPJ.");

        if (request.Dependentes is { } dependentes &&
            (dependentes.Any(d => d.Id != Guid.Empty && !beneficiario.Dependentes.Any(x => x.Id == d.Id)) ||
             dependentes.Where(d => d.Id != Guid.Empty).GroupBy(d => d.Id).Any(g => g.Count() > 1)))
            return BadRequest("Dependente inválido para este beneficiário.");

        SincronizarDependentes(beneficiario, request.Dependentes);
        Aplicar(beneficiario, request, cpf);
        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/inativar")]
    public async Task<IActionResult> Inativar(Guid id, CancellationToken cancellationToken)
    {
        var beneficiario = await context.Beneficiarios.Include(x => x.Dependentes).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (beneficiario is null) return NotFound();

        beneficiario.DefinirStatus(PVHSAUDE.Domain.Enuns.StatusBeneficiario.Inativo);
        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken cancellationToken)
    {
        var beneficiario = await context.Beneficiarios.Include(x => x.Dependentes).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (beneficiario is null) return NotFound();

        context.Beneficiarios.Remove(beneficiario);
        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private void SincronizarDependentes(Beneficiario beneficiario, List<DependenteRequest>? dependentes)
    {
        if (dependentes is null) return;
        foreach (var existente in beneficiario.Dependentes.Where(x => !dependentes.Any(d => d.Id == x.Id)).ToList())
        {
            context.Dependentes.Remove(existente);
            beneficiario.Dependentes.Remove(existente);
        }
        foreach (var item in dependentes)
        {
            if (item.Id == Guid.Empty)
                beneficiario.Dependentes.Add(new Dependente(beneficiario.Id, item.Nome.Trim(), NormalizarCpf(item.Cpf), item.DataNascimento!.Value, item.GrauParentesco!.Value));
            else
                beneficiario.Dependentes.Single(x => x.Id == item.Id).Atualizar(item.Nome.Trim(), NormalizarCpf(item.Cpf), item.DataNascimento!.Value, item.GrauParentesco!.Value);
        }
    }

    private static Beneficiario CriarEntidade(BeneficiarioRequest request, string cpf)
    {
        var beneficiario = new Beneficiario(request.Nome.Trim(), cpf, request.DataNascimento, request.PlanoId, request.DataInicio, request.DataValidade, request.CredenciadoId);
        Aplicar(beneficiario, request, cpf);
        return beneficiario;
    }

    private static void Aplicar(Beneficiario beneficiario, BeneficiarioRequest request, string cpf) =>
        beneficiario.Atualizar(request.Nome.Trim(), cpf, request.DataNascimento, request.Telefone?.Trim(), request.Email?.Trim(), request.Endereco?.Trim(), request.PlanoId, request.DataInicio, request.DataValidade, request.Status, request.CredenciadoId);

    private static string NormalizarCpf(string cpf) => new(cpf.Where(char.IsDigit).ToArray());

    private static BeneficiarioResponse ParaResponse(Beneficiario x) => new(x.Id, x.Nome, x.Cpf, x.DataNascimento, x.Telefone, x.Email, x.Endereco, x.PlanoId, x.DataInicio, x.DataValidade, x.Status,
        x.Dependentes.Select(d => new DependenteResponse(d.Id, d.Nome, d.Cpf, d.DataNascimento, d.GrauParentesco)).ToList(), x.CredenciadoId);
}
