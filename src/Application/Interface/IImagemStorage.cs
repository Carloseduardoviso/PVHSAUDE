namespace PVHSAUDE.Application.Interface;
public interface IImagemStorage
{
    Task<string> SalvarCredenciadoAsync(Guid id, string extensao, Stream conteudo, CancellationToken ct);
}
