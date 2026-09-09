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
            => View(await CarregarRede(cancellationToken));

        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Credenciadas(CancellationToken cancellationToken)
            => View(await CarregarRede(cancellationToken));

        private async Task<PortalViewModel> CarregarRede(CancellationToken cancellationToken)
        {
            var model = new PortalViewModel();
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

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
