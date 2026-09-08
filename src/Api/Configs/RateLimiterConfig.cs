using System.Threading.RateLimiting;

namespace PVHSAUDE.Api.Configs
{
    public static class RateLimiterConfig
    {
        public const string LimiterAccessInfoUser = "LimiterAcessInfoUser";
        public const string LimiterUpdatePassword = "LimiterUpdatePassword";
        public static IServiceCollection AddConfigRatesLimiter(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.AddPolicy(LimiterAccessInfoUser, context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 15,
                            Window = TimeSpan.FromMinutes(2),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        }));

                options.AddPolicy(LimiterUpdatePassword, context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 3,
                            Window = TimeSpan.FromMinutes(5),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        }));
            });

            return services;
        }
    }
}