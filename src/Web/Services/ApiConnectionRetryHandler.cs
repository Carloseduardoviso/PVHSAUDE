using System.Net.Sockets;

namespace Web.Services;

public class ApiConnectionRetryHandler : DelegatingHandler
{
    private readonly TimeSpan retryDelay;
    private readonly int maxAttempts;

    public ApiConnectionRetryHandler(TimeSpan? retryDelay = null, int maxAttempts = 30)
    {
        this.retryDelay = retryDelay ?? TimeSpan.FromSeconds(1);
        this.maxAttempts = maxAttempts;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await base.SendAsync(request, ct);
            }
            catch (HttpRequestException ex) when (attempt < maxAttempts &&
                                                   ex.InnerException is SocketException { SocketErrorCode: SocketError.ConnectionRefused })
            {
                await Task.Delay(retryDelay, ct);
            }
        }
    }
}
