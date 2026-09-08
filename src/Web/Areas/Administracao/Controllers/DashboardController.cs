using Microsoft.AspNetCore.Mvc;

namespace PVHSAUDE.Web.Areas.Administracao.Controllers;

[Area("Administracao")]
public class DashboardController : Controller
{
    public IActionResult Index() => View();
}
