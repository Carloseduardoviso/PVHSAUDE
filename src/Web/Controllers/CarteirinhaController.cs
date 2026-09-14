using Microsoft.AspNetCore.Mvc;

namespace PVHSAUDE.Web.Controllers;

public class CarteirinhaController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();
}
