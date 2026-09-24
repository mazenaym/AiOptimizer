using System.Collections.Concurrent;
using System.Net;

namespace PromptOptimizer.Api.Middleware;

public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly ConcurrentDictionary<string, List<DateTime>> RequestTracker = new();
    private const int MaxRequestsPerMinute = 60;

    public RateLimitMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var now = DateTime.UtcNow;

        var timestamps = RequestTracker.GetOrAdd(clientIp, _ => new List<DateTime>());

        lock (timestamps)
        {
            timestamps.RemoveAll(t => t < now.AddMinutes(-1));

            if (timestamps.Count >= MaxRequestsPerMinute)
            {
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.Headers.RetryAfter = "60";
                return;
            }

            timestamps.Add(now);
        }

        await _next(context);
    }
}
