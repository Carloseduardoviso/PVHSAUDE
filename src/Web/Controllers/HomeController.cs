using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Web.Models;
using Web.Services;
using PVHSAUDE.Domain.Enuns;

namespace Web.Controllers
{
    public class HomeController(CredenciadoApiClient credenciados, IHttpClientFactory clients, ILogger<HomeController> logger) : Controller
    {
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var model = new PortalVm();
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(5));
            try
            {
                model.Banners = await new BannerApiClient(clients.CreateClient("default")).ListarAsync(true, timeout.Token);
                model.BannersLateralEsquerda = model.Banners.Where(x => x.Posicao == PVHSAUDE.Domain.Enuns.PosicaoBanner.LateralEsquerda).ToList();
                model.BannersCentral = model.Banners.Where(x => x.Posicao == PVHSAUDE.Domain.Enuns.PosicaoBanner.Central).ToList();
                model.BannersLateralDireita = model.Banners.Where(x => x.Posicao == PVHSAUDE.Domain.Enuns.PosicaoBanner.LateralDireita).ToList();
                model.BannersInferiorEsquerda = model.Banners.Where(x => x.Posicao == PVHSAUDE.Domain.Enuns.PosicaoBanner.InferiorEsquerda).ToList();
                model.BannersInferiorDireita = model.Banners.Where(x => x.Posicao == PVHSAUDE.Domain.Enuns.PosicaoBanner.InferiorDireita).ToList();
            }
            catch (Exception ex) when (ex is HttpRequestException or System.Text.Json.JsonException || ex is OperationCanceledException && !cancellationToken.IsCancellationRequested)
            { logger.LogWarning(ex, "Não foi possível carregar os banners do portal."); }
            return View(model);
        }

        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> BannerImagem(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var imagem = await new BannerApiClient(clients.CreateClient("default")).ImagemAsync(id, cancellationToken);
                if (imagem is null) return NotFound();
                Response.Headers.XContentTypeOptions = "nosniff";
                return File(imagem.Value.Bytes, imagem.Value.Tipo);
            }
            catch (HttpRequestException) { return StatusCode(503); }
        }

        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Credenciadas(CancellationToken cancellationToken)
            => View(await CarregarRede(cancellationToken));

        private async Task<PortalVm> CarregarRede(CancellationToken cancellationToken)
        {
            var model = new PortalVm();
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(5));
            try
            {
                var empresas = await credenciados.ListarAsync(timeout.Token);
                model.Empresas = empresas.Where(x => x.StatusCredenciamento == StatusCredenciamento.Ativo).ToList();
                model.Especialidades = await credenciados.EspecialidadesAsync(timeout.Token);
                model.Procedimentos = await credenciados.ProcedimentosAsync(timeout.Token);
            }
            catch (Exception ex) when (ex is HttpRequestException or System.Text.Json.JsonException ||
                                       ex is OperationCanceledException && !cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning(ex, "Não foi possível carregar a rede credenciada no portal.");
                model.ErroCatalogos = "Não foi possível carregar todas as informações da rede. Tente novamente em instantes.";
            }
            using var planosTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            planosTimeout.CancelAfter(TimeSpan.FromSeconds(5));
            try
            {
                using var client = clients.CreateClient("default");
                model.Planos = await client.GetFromJsonAsync<List<EmpresaPlanoPortal>>(
                    "api/portal/planos-empresas", planosTimeout.Token) ?? [];
            }
            catch (Exception ex) when (ex is HttpRequestException or System.Text.Json.JsonException ||
                                       ex is OperationCanceledException && !cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning(ex, "Falha na consulta dos planos das empresas no portal.");
                model.ErroPlanos = "Valores temporariamente indisponíveis.";
            }
            return model;
        }

        public async Task<IActionResult> ClinicaTerapeutica(CancellationToken ct) => await MenuCredenciado(TipoCredenciado.Clinica, "Clínica terapêutica Parceiras", ct);
        public async Task<IActionResult> ClinicaTerapeuticaCredenciada(CancellationToken ct) => await MenuCredenciado(TipoCredenciado.ClinicaTerapeuticaCredenciada, "Clínica terapêutica credenciada", ct);
        public async Task<IActionResult> ClinicaMedicaTerapeutica(CancellationToken ct) => await MenuCredenciado(TipoCredenciado.Laboratorio, "Clínica Médica e Especialidade", ct);
        public async Task<IActionResult> LaboratorioExames(CancellationToken ct) => await MenuCredenciado(TipoCredenciado.Hospital, "Laboratório e Exames", ct);
        public async Task<IActionResult> ExamesImagens(CancellationToken ct) => await MenuCredenciado(TipoCredenciado.CentroDiagnostico, "Exames e Imagens", ct);
        public async Task<IActionResult> Odontologia(CancellationToken ct) => await MenuCredenciado(TipoCredenciado.Farmacia, "Odontologia", ct);
        public async Task<IActionResult> ClinicasPopulares(CancellationToken ct) => await MenuCredenciado(TipoCredenciado.ClinicasPopulares, "Clínicas Populares", ct);
        public async Task<IActionResult> Academia(CancellationToken ct) => await MenuCredenciado(TipoCredenciado.Academia, "Academia", ct);
        public async Task<IActionResult> Futebol(CancellationToken ct) => await MenuCredenciado(TipoCredenciado.Futebol, "Futebol", ct);
        public async Task<IActionResult> Natacao(CancellationToken ct) => await MenuCredenciado(TipoCredenciado.Natacao, "Natação", ct);
        public async Task<IActionResult> Otica(CancellationToken ct) => await MenuCredenciado(TipoCredenciado.Otica, "Ótica", ct);
        public async Task<IActionResult> RoupaEsportiva(CancellationToken ct) => await MenuCredenciado(TipoCredenciado.RoupaEsportiva, "Roupa Esportiva", ct);

        private async Task<IActionResult> MenuCredenciado(TipoCredenciado tipo, string titulo, CancellationToken ct)
        {
            var model = await CarregarRede(ct);
            model.Empresas = model.Empresas.Where(x => x.Tipo == tipo).ToList();
            ViewData["Title"] = titulo;
            return View("Credenciadas", model);
        }

        public IActionResult SobreNos() => View();

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorVm { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
