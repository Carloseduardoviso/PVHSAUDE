using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using Microsoft.Extensions.Logging.Abstractions;
using PVHSAUDE.Application.AppService;
using PVHSAUDE.Application.AutoMapper;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Domain.Entities;
using PVHSAUDE.Domain.Interfaces.Repository;
using System.Linq.Expressions;

internal static class CatalogoTests
{
    public static async Task Run()
    {
        var config = new MapperConfiguration(c =>
        {
            c.AddExpressionMapping();
            c.AddProfile<AutoMapperConfig>();
        }, NullLoggerFactory.Instance);
        var mapper = config.CreateMapper();
        var especialidade = new Especialidade("Original");
        var procedimento = new Procedimento("Original");
        var especialidades = new FakeRepository<Especialidade>(especialidade);
        var procedimentos = new FakeRepository<Procedimento>(procedimento);
        var work = new FakeUnitOfWork();
        var service = new CatalogoService(especialidades, procedimentos, work, mapper);

        await service.AtualizarEspecialidadeAsync(especialidade.Id, new CatalogoEntradaVm(" Editada "), default);
        Check(especialidade.Nome == "Editada" && especialidade.Ativo, "edição de especialidade atualiza o nome");
        await service.ExcluirEspecialidadeAsync(especialidade.Id, default);
        Check(!especialidade.Ativo, "exclusão de especialidade é lógica");

        await service.AtualizarProcedimentoAsync(procedimento.Id, new CatalogoEntradaVm(" Editado "), default);
        Check(procedimento.Nome == "Editado" && procedimento.Ativo, "edição de procedimento atualiza o nome");
        await service.ExcluirProcedimentoAsync(procedimento.Id, default);
        Check(!procedimento.Ativo, "exclusão de procedimento é lógica");
    }

    private static void Check(bool value, string message)
    {
        if (!value) throw new Exception("Catálogo: " + message);
        Console.WriteLine("PASS: Catálogo - " + message);
    }

    private sealed class FakeRepository<T>(T item) : IEntityRepository<T> where T : class
    {
        private readonly List<T> _items = [item];
        public Task<List<T>> ListarAsync(Expression<Func<T, bool>>? filtro, CancellationToken ct, params Expression<Func<T, object>>[] includes) =>
            Task.FromResult(filtro is null ? _items : _items.AsQueryable().Where(filtro).ToList());
        public Task<T?> ObterAsync(Expression<Func<T, bool>> filtro, CancellationToken ct, params Expression<Func<T, object>>[] includes) =>
            Task.FromResult(_items.AsQueryable().FirstOrDefault(filtro));
        public Task<bool> ExisteAsync(Expression<Func<T, bool>> filtro, CancellationToken ct) => Task.FromResult(_items.AsQueryable().Any(filtro));
        public void Adicionar(T entidade) => _items.Add(entidade);
        public void Remover(T entidade) => _items.Remove(entidade);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task SalvarAsync(CancellationToken ct) => Task.CompletedTask;
        public Task<IRepositoryTransaction> BloquearUsuariosAsync(CancellationToken ct) => throw new NotSupportedException();
    }
}
