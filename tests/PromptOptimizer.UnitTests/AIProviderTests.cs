using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PromptOptimizer.Application.Common.Exceptions;
using PromptOptimizer.Application.Common.Interfaces;
using PromptOptimizer.Application.Optimization.Models;
using PromptOptimizer.Application.Optimization.Services;
using PromptOptimizer.Domain.ValueObjects;
using PromptOptimizer.Infrastructure;
using PromptOptimizer.Infrastructure.AI;
using PromptOptimizer.Infrastructure.AI.Gemini;
using PromptOptimizer.Infrastructure.AI.Ollama;
using PromptOptimizer.Infrastructure.AI.OpenRouter;

namespace PromptOptimizer.UnitTests;

public class AIProviderTests
{
    public static TheoryData<string> Providers => new() { "Gemini", "OpenRouter", "Ollama" };

    private static IConfiguration Configuration(string provider, string? key = "test-secret", string? baseUrl = null)
    {
        var values = new Dictionary<string, string?> { [$"AI:{provider}:ApiKey"] = key };
        if (baseUrl is not null) values[$"AI:{provider}:BaseUrl"] = baseUrl;
        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }

    private static IAIService Service(string provider, HttpClient client, IConfiguration? configuration = null) => provider switch
    {
        "Gemini" => new GeminiService(client, configuration ?? Configuration(provider)),
        "OpenRouter" => new OpenRouterService(client, configuration ?? Configuration(provider)),
        "Ollama" => new OllamaService(client, configuration ?? Configuration(provider)),
        _ => throw new ArgumentException(nameof(provider))
    };

    private static string Success(string provider, bool usage = true) => provider switch
    {
        "Gemini" => "{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"optimized\"}]},\"finishReason\":\"STOP\"}]"
            + (usage ? ",\"usageMetadata\":{\"promptTokenCount\":12,\"candidatesTokenCount\":7}" : "") + "}",
        "OpenRouter" => "{\"choices\":[{\"message\":{\"content\":\"optimized\"}}]"
            + (usage ? ",\"usage\":{\"prompt_tokens\":12,\"completion_tokens\":7}" : "") + "}",
        _ => "{\"response\":\"optimized\",\"done\":true"
            + (usage ? ",\"prompt_eval_count\":12,\"eval_count\":7" : "") + "}"
    };

    [Theory]
    [MemberData(nameof(Providers))]
    public void Infrastructure_resolves_each_provider_without_optional_configuration(string provider)
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddInfrastructureServices(configuration);
        using var container = services.BuildServiceProvider();
        using var scope = container.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IAIServiceFactory>();
        Assert.Equal(provider, factory.GetService(provider.ToLowerInvariant()).ProviderName);
        Assert.Equal(3, scope.ServiceProvider.GetServices<IAIService>().Count());
    }

    [Fact]
    public void Factory_rejects_unknown_empty_and_duplicate_registrations()
    {
        using var client = new HttpClient();
        var service = Service("Gemini", client);
        AssertConfiguration(() => new AIServiceFactory([service]).GetService("unknown"));
        AssertConfiguration(() => new AIServiceFactory([]).GetService("Gemini"));
        AssertConfiguration(() => new AIServiceFactory([service, service]).GetService("Gemini"));
        AssertConfiguration(() => new AIServiceFactory([service]).GetService(""));
    }

    private static void AssertConfiguration(Action action) =>
        Assert.Equal(AIProviderFailure.Configuration, Assert.Throws<AIProviderException>(action).Failure);

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task Success_parses_usage_and_disposes_response(string provider)
    {
        var content = new TrackingContent(Success(provider));
        using var client = new HttpClient(new StubHandler((request, _) =>
        {
            Assert.DoesNotContain("test-secret", request.RequestUri!.ToString());
            Assert.StartsWith("https://example.test/custom/", request.RequestUri.ToString());
            if (provider == "Gemini") Assert.Equal("test-secret", request.Headers.GetValues("x-goog-api-key").Single());
            if (provider == "OpenRouter") Assert.Equal("Bearer test-secret", request.Headers.GetValues("Authorization").Single());
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        }));
        var result = await Service(provider, client, Configuration(provider, baseUrl: "https://example.test/custom"))
            .OptimizePromptAsync("original", "instructions", "model");
        Assert.Equal("optimized", result.OptimizedContent);
        Assert.Equal(new TokenUsage(12, 7), result.TokenUsage);
        Assert.Equal(19, result.TokenUsage.TotalTokens);
        Assert.True(content.Disposed);
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task Missing_usage_is_unknown(string provider)
    {
        using var client = Client(Success(provider, false));
        var result = await Service(provider, client).OptimizePromptAsync("original", "instructions", "model");
        Assert.Equal(TokenUsage.Unknown, result.TokenUsage);
        Assert.Null(result.TokenUsage.TotalTokens);
    }

    [Theory]
    [InlineData("Gemini")]
    [InlineData("OpenRouter")]
    public async Task Missing_key_fails_only_when_used_without_http(string provider)
    {
        using var client = new HttpClient(new StubHandler((_, _) => throw new Xunit.Sdk.XunitException("Unexpected HTTP call")));
        var service = Service(provider, client, Configuration(provider, null));
        var error = await Assert.ThrowsAsync<AIProviderException>(() => service.OptimizePromptAsync("original", "instructions", "model"));
        Assert.Equal(AIProviderFailure.Configuration, error.Failure);
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task Invalid_endpoint_is_configuration_failure(string provider)
    {
        using var client = new HttpClient();
        foreach (var endpoint in new[] { "", "not-a-url", "https://example.test/?key=test-secret", "https://user:password@example.test" })
        {
            var error = await Assert.ThrowsAsync<AIProviderException>(() => Service(provider, client,
                Configuration(provider, baseUrl: endpoint)).OptimizePromptAsync("original", "instructions", "model"));
            Assert.Equal(AIProviderFailure.Configuration, error.Failure);
            Assert.DoesNotContain("test-secret", error.ToString());
        }
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task Http_failures_are_classified_sanitized_and_disposed(string provider)
    {
        foreach (var (status, expected) in new[]
        {
            (401, AIProviderFailure.Authentication), (403, AIProviderFailure.Authentication),
            (429, AIProviderFailure.RateLimit), (408, AIProviderFailure.Unavailable),
            (503, AIProviderFailure.Unavailable), (400, AIProviderFailure.InvalidResponse)
        })
        {
            var content = new TrackingContent("secret-upstream-body");
            using var client = new HttpClient(new StubHandler((_, _) => Task.FromResult(
                new HttpResponseMessage((HttpStatusCode)status) { Content = content })));
            var error = await Assert.ThrowsAsync<AIProviderException>(() => Service(provider, client)
                .OptimizePromptAsync("original", "instructions", "model"));
            Assert.Equal(expected, error.Failure);
            Assert.DoesNotContain("secret", error.ToString());
            Assert.True(content.Disposed);
        }
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task Invalid_responses_never_become_success(string provider)
    {
        foreach (var body in new[] { "", "not json", "null", "{}", "[]", "{\"choices\":[],\"candidates\":[],\"response\":\"\",\"done\":true}",
            Success(provider).Replace("optimized", " "), Success(provider).Replace(":12", ":-1") })
        {
            using var client = Client(body);
            var error = await Assert.ThrowsAsync<AIProviderException>(() => Service(provider, client)
                .OptimizePromptAsync("original", "instructions", "model"));
            Assert.Equal(AIProviderFailure.InvalidResponse, error.Failure);
        }
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task Error_envelope_is_not_success_even_with_content(string provider)
    {
        using var client = Client(Success(provider).Insert(1, "\"error\":{\"message\":\"secret\"},"));
        var error = await Assert.ThrowsAsync<AIProviderException>(() => Service(provider, client)
            .OptimizePromptAsync("original", "instructions", "model"));
        Assert.Equal(AIProviderFailure.InvalidResponse, error.Failure);
        Assert.DoesNotContain("secret", error.ToString());
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task Partial_usage_keeps_missing_count_unknown(string provider)
    {
        var body = Success(provider).Replace("\"completion_tokens\":7", "\"completion_tokens\":null")
            .Replace("\"candidatesTokenCount\":7", "\"candidatesTokenCount\":null")
            .Replace("\"eval_count\":7", "\"eval_count\":null");
        using var client = Client(body);
        var result = await Service(provider, client).OptimizePromptAsync("original", "instructions", "model");
        Assert.Equal(12, result.TokenUsage.PromptTokens);
        Assert.Null(result.TokenUsage.CompletionTokens);
        Assert.Null(result.TokenUsage.TotalTokens);
    }

    [Theory]
    [InlineData("{\"promptFeedback\":{\"blockReason\":\"SAFETY\"}}")]
    [InlineData("{\"candidates\":[{\"finishReason\":\"SAFETY\",\"content\":{\"parts\":[{\"text\":\"blocked\"}]}}]}")]
    [InlineData("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"private thought\",\"thought\":true}]}}]}")]
    public async Task Gemini_rejects_blocked_or_missing_output(string body)
    {
        using var client = Client(body);
        var error = await Assert.ThrowsAsync<AIProviderException>(() => Service("Gemini", client)
            .OptimizePromptAsync("original", "instructions", "model"));
        Assert.Equal(AIProviderFailure.InvalidResponse, error.Failure);
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task Caller_cancellation_propagates(string provider)
    {
        using var cancellation = new CancellationTokenSource();
        using var client = new HttpClient(new StubHandler((_, token) =>
        {
            Assert.True(token.CanBeCanceled);
            cancellation.Cancel();
            token.ThrowIfCancellationRequested();
            throw new Xunit.Sdk.XunitException("Cancellation was lost");
        }));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Service(provider, client)
            .OptimizePromptAsync("original", "instructions", "model", cancellation.Token));
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task Timeout_and_transport_failure_are_sanitized(string provider)
    {
        foreach (var exception in new Exception[] { new TaskCanceledException("secret-url"), new HttpRequestException("secret-url") })
        {
            using var client = new HttpClient(new StubHandler((_, _) => throw exception));
            var error = await Assert.ThrowsAsync<AIProviderException>(() => Service(provider, client)
                .OptimizePromptAsync("original", "instructions", "model"));
            Assert.Equal(AIProviderFailure.Unavailable, error.Failure);
            Assert.DoesNotContain("secret", error.ToString());
            Assert.Null(error.InnerException);
        }
    }

    [Fact]
    public async Task Engine_uses_selected_provider_with_instructions()
    {
        using var client = new HttpClient(new StubHandler(async (request, token) =>
        {
            var body = await request.Content!.ReadAsStringAsync(token);
            Assert.Contains("custom instructions", body);
            Assert.Contains("original", body);
            Assert.Contains("chosen-model", body);
            Assert.Contains("General", body);
            Assert.Contains("English", body);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(Success("OpenRouter")) };
        }));
        var engine = new PromptOptimizerEngine(new AIServiceFactory([Service("OpenRouter", client)]));
        var result = await engine.OptimizeAsync(new PromptOptimizationRequest("original", "General", "English",
            "openrouter", "chosen-model", "custom instructions"));
        Assert.Equal("optimized", result.OptimizedContent);
    }

    [Fact]
    public void Usage_preserves_partial_unknown_and_known_zero()
    {
        Assert.Null(new TokenUsage(4, null).TotalTokens);
        Assert.Equal(new TokenUsage(4, null), new TokenUsage(4, null).Add(TokenUsage.Empty));
        Assert.Equal(0, TokenUsage.Empty.TotalTokens);
    }

    private static HttpClient Client(string body) => new(new StubHandler((_, _) => Task.FromResult(
        new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body) })));

    private sealed class StubHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => send(request, cancellationToken);
    }

    private sealed class TrackingContent(string value) : StringContent(value)
    {
        public bool Disposed { get; private set; }
        protected override void Dispose(bool disposing) { Disposed = true; base.Dispose(disposing); }
    }
}
