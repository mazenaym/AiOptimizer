using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PromptOptimizer.Application.Common.Exceptions;
using PromptOptimizer.Application.Common.Interfaces;
using PromptOptimizer.Domain.ValueObjects;
using static PromptOptimizer.Infrastructure.AI.ProviderHttp;

namespace PromptOptimizer.Infrastructure.AI.Ollama;

public class OllamaService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    public string ProviderName => "Ollama";

    public OllamaService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<AIOptimizationResponse> OptimizePromptAsync(string originalPrompt, string systemInstructions,
        string modelKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var stopwatch = Stopwatch.StartNew();
        using var request = new HttpRequestMessage(HttpMethod.Post,
            Endpoint(_configuration["AI:Ollama:BaseUrl"] ?? "http://localhost:11434", "api/generate"));
        request.Content = JsonContent.Create(new
        {
            model = string.IsNullOrWhiteSpace(modelKey) ? "llama3" : modelKey,
            system = systemInstructions,
            prompt = $"Optimize this prompt:\n{originalPrompt}",
            stream = false
        });
        return await SendAsync(_httpClient, request, json =>
        {
            if (Property(json, "error").ValueKind is not (JsonValueKind.Undefined or JsonValueKind.Null)
                || Property(json, "done").ValueKind != JsonValueKind.True)
                throw new AIProviderException(AIProviderFailure.InvalidResponse);
            var content = RequireText(Text(Property(json, "response")));
            return new AIOptimizationResponse(content, "Optimized locally via Ollama.",
                new TokenUsage(Count(json, "prompt_eval_count"), Count(json, "eval_count")),
                stopwatch.Elapsed.TotalMilliseconds);
        }, cancellationToken);
    }
}
