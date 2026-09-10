using Microsoft.AspNetCore.Mvc;

namespace PVHSAUDE.Web.Controllers;

public class SportsController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult Academia() => View();

    [HttpGet]
    public IActionResult Futebol() => View();

    [HttpGet]
    public IActionResult Natacao() => View();
}
