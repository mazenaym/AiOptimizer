using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PromptOptimizer.Application.Common.Exceptions;
using PromptOptimizer.Application.Common.Interfaces;
using PromptOptimizer.Domain.ValueObjects;
using static PromptOptimizer.Infrastructure.AI.ProviderHttp;

namespace PromptOptimizer.Infrastructure.AI.OpenRouter;

public class OpenRouterService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    public string ProviderName => "OpenRouter";

    public OpenRouterService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<AIOptimizationResponse> OptimizePromptAsync(string originalPrompt, string systemInstructions,
        string modelKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var key = _configuration["AI:OpenRouter:ApiKey"] ?? string.Empty;
        RequireKey(key);
        var stopwatch = Stopwatch.StartNew();
        using var request = new HttpRequestMessage(HttpMethod.Post,
            Endpoint(_configuration["AI:OpenRouter:BaseUrl"] ?? "https://openrouter.ai/api/v1", "chat/completions"));
        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {key}");
        request.Content = JsonContent.Create(new
        {
            model = string.IsNullOrWhiteSpace(modelKey) ? "openai/gpt-4o-mini" : modelKey,
            messages = new[]
            {
                new { role = "system", content = systemInstructions },
                new { role = "user", content = $"Please optimize the following prompt for best results:\n\n{originalPrompt}" }
            }
        });
        return await SendAsync(_httpClient, request, json =>
        {
            if (Property(json, "error").ValueKind is not (JsonValueKind.Undefined or JsonValueKind.Null))
                throw new AIProviderException(AIProviderFailure.InvalidResponse);
            var message = Property(First(Property(json, "choices")), "message");
            var content = RequireText(Text(Property(message, "content")));
            var usage = Property(json, "usage");
            return new AIOptimizationResponse(content, "Optimized prompt using OpenRouter API.",
                new TokenUsage(Count(usage, "prompt_tokens"), Count(usage, "completion_tokens")),
                stopwatch.Elapsed.TotalMilliseconds);
        }, cancellationToken);
    }
}
