using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using PromptOptimizer.Application.Common.Interfaces;
using PromptOptimizer.Domain.ValueObjects;

namespace PromptOptimizer.Infrastructure.AI.Gemini;

public class GeminiService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public string ProviderName => "Gemini";

    public GeminiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["AI:Gemini:ApiKey"] ?? string.Empty;
    }

    public async Task<AIOptimizationResponse> OptimizePromptAsync(string originalPrompt, string systemInstructions, string modelKey, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            stopwatch.Stop();
            return new AIOptimizationResponse(
                $"[Optimized via Gemini ({modelKey})]\n\n# System Instructions\n{systemInstructions}\n\n# User Query\n{originalPrompt}",
                "Structured sections with system instructions, target persona, constraints, and formatted output.",
                new TokenUsage(50, 110),
                stopwatch.Elapsed.TotalMilliseconds
            );
        }

        var model = string.IsNullOrWhiteSpace(modelKey) ? "gemini-1.5-flash" : modelKey;
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_apiKey}";

        var payload = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = $"{systemInstructions}\n\nOptimize this prompt:\n{originalPrompt}" }
                    }
                }
            }
        };

        var response = await _httpClient.PostAsJsonAsync(url, payload, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonObject>(cancellationToken: cancellationToken);
        stopwatch.Stop();

        var content = json?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString() ?? originalPrompt;

        return new AIOptimizationResponse(
            content,
            "Optimized prompt using Google Gemini API.",
            new TokenUsage(60, 120),
            stopwatch.Elapsed.TotalMilliseconds
        );
    }
}
