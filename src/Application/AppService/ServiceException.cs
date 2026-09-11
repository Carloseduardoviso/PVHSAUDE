namespace PVHSAUDE.Application.AppService;

public enum ServiceError { NotFound, Invalid, Validation, Conflict, Unauthorized }
public sealed class ServiceException(ServiceError error, string? message = null, bool problem = false) : Exception(message)
{
    public ServiceError Error { get; } = error;
    public bool Problem { get; } = problem;
}
