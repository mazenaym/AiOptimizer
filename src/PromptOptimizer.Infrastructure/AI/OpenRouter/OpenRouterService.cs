using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using PromptOptimizer.Application.Common.Interfaces;
using PromptOptimizer.Domain.ValueObjects;

namespace PromptOptimizer.Infrastructure.AI.OpenRouter;

public class OpenRouterService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public string ProviderName => "OpenRouter";

    public OpenRouterService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["AI:OpenRouter:ApiKey"] ?? string.Empty;
    }

    public async Task<AIOptimizationResponse> OptimizePromptAsync(string originalPrompt, string systemInstructions, string modelKey, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            stopwatch.Stop();
            // Mock fallback response for demonstration when API key is missing
            return new AIOptimizationResponse(
                $"[Optimized via OpenRouter ({modelKey})]\n\nRole: Expert Prompt Assistant\nGoal: {originalPrompt}\nFormat: Structured JSON / Step-by-Step",
                "Enhanced role specification, specified output format, and added context guidelines.",
                new TokenUsage(45, 90),
                stopwatch.Elapsed.TotalMilliseconds
            );
        }

        var payload = new
        {
            model = string.IsNullOrWhiteSpace(modelKey) ? "openai/gpt-4o-mini" : modelKey,
            messages = new[]
            {
                new { role = "system", content = systemInstructions },
                new { role = "user", content = $"Please optimize the following prompt for best results:\n\n{originalPrompt}" }
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://openrouter.ai/api/v1/chat/completions");
        request.Headers.Add("Authorization", $"Bearer {_apiKey}");
        request.Content = JsonContent.Create(payload);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonObject>(cancellationToken: cancellationToken);
        stopwatch.Stop();

        var content = json?["choices"]?[0]?["message"]?["content"]?.ToString() ?? originalPrompt;
        var promptTokens = json?["usage"]?["prompt_tokens"]?.GetValue<int>() ?? 0;
        var completionTokens = json?["usage"]?.AsObject().ContainsKey("completion_tokens") == true ? json["usage"]!["completion_tokens"]!.GetValue<int>() : 0;

        return new AIOptimizationResponse(
            content,
            "Optimized prompt using OpenRouter API.",
            new TokenUsage(promptTokens, completionTokens),
            stopwatch.Elapsed.TotalMilliseconds
        );
    }
}
