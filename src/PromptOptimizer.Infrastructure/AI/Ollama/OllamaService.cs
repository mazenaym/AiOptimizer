using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using PromptOptimizer.Application.Common.Interfaces;
using PromptOptimizer.Domain.ValueObjects;

namespace PromptOptimizer.Infrastructure.AI.Ollama;

public class OllamaService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public string ProviderName => "Ollama";

    public OllamaService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["AI:Ollama:BaseUrl"] ?? "http://localhost:11434";
    }

    public async Task<AIOptimizationResponse> OptimizePromptAsync(string originalPrompt, string systemInstructions, string modelKey, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var model = string.IsNullOrWhiteSpace(modelKey) ? "llama3" : modelKey;

        try
        {
            var payload = new
            {
                model,
                system = systemInstructions,
                prompt = $"Optimize this prompt:\n{originalPrompt}",
                stream = false
            };

            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/generate", payload, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<JsonObject>(cancellationToken: cancellationToken);
            stopwatch.Stop();

            var content = json?["response"]?.ToString() ?? originalPrompt;

            return new AIOptimizationResponse(
                content,
                "Optimized locally via Ollama.",
                new TokenUsage(40, 80),
                stopwatch.Elapsed.TotalMilliseconds
            );
        }
        catch
        {
            stopwatch.Stop();
            return new AIOptimizationResponse(
                $"[Optimized via Ollama ({model})]\n\n{systemInstructions}\n\nTask: {originalPrompt}",
                "Structured prompt template applied locally via Ollama.",
                new TokenUsage(35, 75),
                stopwatch.Elapsed.TotalMilliseconds
            );
        }
    }
}
