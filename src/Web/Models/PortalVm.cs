namespace Web.Models;

public class PortalVm
{
    public List<BannerVm> Banners { get; set; } = [];
    public List<BannerVm> BannersLateralEsquerda { get; set; } = [];
    public List<BannerVm> BannersCentral { get; set; } = [];
    public List<BannerVm> BannersLateralDireita { get; set; } = [];
    public List<BannerVm> BannersInferiorEsquerda { get; set; } = [];
    public List<BannerVm> BannersInferiorDireita { get; set; } = [];
    public List<CredenciadoVm> Empresas { get; set; } = [];
    public IReadOnlyCollection<CatalogoItemVm> Especialidades { get; set; } = [];
    public IReadOnlyCollection<CatalogoItemVm> Procedimentos { get; set; } = [];
    public string? ErroCatalogos { get; set; }
    public string? ErroPlanos { get; set; }
    public List<EmpresaPlanoPortal> Planos { get; set; } = [];
}

public record EmpresaPlanoPortal(Guid CredenciadoId, Guid PlanoId, string Plano, decimal Valor, PVHSAUDE.Domain.Enuns.Periodicidade Periodicidade);
