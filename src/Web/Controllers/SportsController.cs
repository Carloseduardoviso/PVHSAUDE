using Microsoft.AspNetCore.Mvc;

namespace PVHSAUDE.Web.Controllers;

public class SportsController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();
}
