using Microsoft.AspNetCore.Mvc;

namespace PVHSAUDE.Web.Controllers;

public class LojasController : Controller
{
    [HttpGet]
    public IActionResult Otica() => View();

    [HttpGet]
    public IActionResult RoupaEsportiva() => View();
}