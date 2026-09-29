using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PromptOptimizer.Api.Middleware;
using PromptOptimizer.Application.Common.Exceptions;

namespace PromptOptimizer.IntegrationTests;

public class ExceptionMiddlewareTests
{
    [Theory]
    [InlineData(AIProviderFailure.Configuration, 503)]
    [InlineData(AIProviderFailure.Authentication, 503)]
    [InlineData(AIProviderFailure.RateLimit, 429)]
    [InlineData(AIProviderFailure.Unavailable, 503)]
    [InlineData(AIProviderFailure.InvalidResponse, 502)]
    public async Task Provider_errors_use_safe_payload_and_logs(AIProviderFailure failure, int status)
    {
        var logger = new RecordingLogger();
        var error = new AIProviderException(failure,
            "API_KEY=private-key Authorization: Bearer private-token https://upstream.test/?key=private-key raw-provider-body internal-stack-trace");
        var middleware = new ExceptionMiddleware(_ => throw error, logger);
        var context = new DefaultHttpContext { TraceIdentifier = "test-trace" };
        using var stream = new MemoryStream();
        context.Response.Body = stream;
        await middleware.InvokeAsync(context);
        Assert.Equal(status, context.Response.StatusCode);
        Assert.False(context.Response.Headers.ContainsKey("WWW-Authenticate"));
        stream.Position = 0;
        var payload = await new StreamReader(stream).ReadToEndAsync();
        using var json = JsonDocument.Parse(payload);
        Assert.Equal(status, json.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("test-trace", json.RootElement.GetProperty("traceId").GetString());
        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("errors").ValueKind);
        Assert.Equal(4, json.RootElement.EnumerateObject().Count());
        var logs = string.Join("\n", logger.Messages);
        foreach (var secret in new[] { "private-key", "private-token", "Authorization", "upstream.test", "raw-provider-body", "internal-stack-trace" })
        {
            Assert.DoesNotContain(secret, payload);
            Assert.DoesNotContain(secret, logs);
        }
        Assert.Single(logger.Messages);
    }

    [Fact]
    public async Task Request_abort_propagates_without_error_payload_or_logging()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var logger = new RecordingLogger();
        var expected = new OperationCanceledException(cancellation.Token);
        var middleware = new ExceptionMiddleware(_ => throw expected, logger);
        var context = new DefaultHttpContext { RequestAborted = cancellation.Token };
        using var stream = new MemoryStream();
        context.Response.Body = stream;
        var actual = await Assert.ThrowsAsync<OperationCanceledException>(() => middleware.InvokeAsync(context));
        Assert.Same(expected, actual);
        Assert.Equal(0, stream.Length);
        Assert.Empty(logger.Messages);
    }

    private sealed class RecordingLogger : ILogger<ExceptionMiddleware>
    {
        public List<string> Messages { get; } = [];
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter) => Messages.Add(formatter(state, exception) + exception);
    }
}
