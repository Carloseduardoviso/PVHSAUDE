namespace PVHSAUDE.Api.Middleware
{
    public class ValidateTokenForgotPasswordMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ValidateTokenForgotPasswordMiddleware> _logger;

        public ValidateTokenForgotPasswordMiddleware(RequestDelegate next, ILogger<ValidateTokenForgotPasswordMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var user = context.User;
            if (user.Identity?.IsAuthenticated == true && user.HasClaim(c => c.Type == "Purpose" && c.Value == "ForgotPassword"))
            {
                throw new UnauthorizedAccessException();
            }

            await _next(context);
        }

    }
    public static class ValidateTokenForgotPassword
    {
        public static IApplicationBuilder UseValidateTokenForgotPasswordMiddleware(this IApplicationBuilder builder) => builder.UseMiddleware<ValidateTokenForgotPasswordMiddleware>();
    }
}