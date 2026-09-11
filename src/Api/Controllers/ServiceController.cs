using Microsoft.AspNetCore.Mvc;
using PVHSAUDE.Application.AppService;
namespace PVHSAUDE.Api.Controllers;

public abstract class ServiceController : ControllerBase
{
    protected async Task<IActionResult> Executar(Func<Task<IActionResult>> action)
    {
        try { return await action(); }
        catch (ServiceException ex)
        {
            return ex.Error switch
            {
                ServiceError.NotFound => NotFound(),
                ServiceError.Unauthorized => Unauthorized(),
                ServiceError.Validation => ValidationProblem(ex.Message),
                ServiceError.Conflict when ex.Problem => Conflict(new ProblemDetails { Detail = ex.Message }),
                ServiceError.Conflict => Conflict(ex.Message),
                _ => BadRequest(ex.Message)
            };
        }
    }
}
