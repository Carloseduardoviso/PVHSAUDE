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
            var model = await CarregarRede(cancellationToken);
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(5));
            try
            {
                model.Banners = await new BannerApiClient(clients.CreateClient("default")).ListarAsync(true, timeout.Token);
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

        public IActionResult ClinicaTerapeutica() => View();

        public IActionResult ClinicaMedicaTerapeutica() => View();

        public IActionResult LaboratorioExames() => View();

        public IActionResult Odontologia() => View();

        public IActionResult ClinicasPopulares() => View();

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
