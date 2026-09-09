namespace Web.Models;

public class PortalViewModel
{
    public List<BannerViewModel> Banners { get; set; } = [];
    public List<CredenciadoViewModel> Empresas { get; set; } = [];
    public IReadOnlyCollection<CatalogoItemViewModel> Especialidades { get; set; } = [];
    public IReadOnlyCollection<CatalogoItemViewModel> Procedimentos { get; set; } = [];
    public string? ErroCatalogos { get; set; }
    public string? ErroPlanos { get; set; }
    public List<EmpresaPlanoPortal> Planos { get; set; } = [];
}

public record EmpresaPlanoPortal(Guid CredenciadoId, Guid PlanoId, string Plano, decimal Valor, PVHSAUDE.Domain.Enuns.Periodicidade Periodicidade);
