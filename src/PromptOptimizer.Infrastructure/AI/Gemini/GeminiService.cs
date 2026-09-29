using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PromptOptimizer.Application.Common.Exceptions;
using PromptOptimizer.Application.Common.Interfaces;
using PromptOptimizer.Domain.ValueObjects;
using static PromptOptimizer.Infrastructure.AI.ProviderHttp;

namespace PromptOptimizer.Infrastructure.AI.Gemini;

public class GeminiService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    public string ProviderName => "Gemini";

    public GeminiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<AIOptimizationResponse> OptimizePromptAsync(string originalPrompt, string systemInstructions,
        string modelKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var key = _configuration["AI:Gemini:ApiKey"] ?? string.Empty;
        RequireKey(key);
        var stopwatch = Stopwatch.StartNew();
        var model = string.IsNullOrWhiteSpace(modelKey) ? "gemini-1.5-flash" : modelKey;
        using var request = new HttpRequestMessage(HttpMethod.Post,
            Endpoint(_configuration["AI:Gemini:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta",
                $"models/{Uri.EscapeDataString(model)}:generateContent"));
        request.Headers.TryAddWithoutValidation("x-goog-api-key", key);
        request.Content = JsonContent.Create(new
        {
            contents = new[] { new { parts = new[] { new { text = $"{systemInstructions}\n\nOptimize this prompt:\n{originalPrompt}" } } } }
        });
        return await SendAsync(_httpClient, request, json =>
        {
            if (Property(json, "error").ValueKind is not (JsonValueKind.Undefined or JsonValueKind.Null))
                throw new AIProviderException(AIProviderFailure.InvalidResponse);
            var feedback = Property(json, "promptFeedback");
            var blocked = Text(Property(feedback, "blockReason"));
            var candidate = First(Property(json, "candidates"));
            var finish = Text(Property(candidate, "finishReason"));
            if ((!string.IsNullOrEmpty(blocked) && blocked != "BLOCK_REASON_UNSPECIFIED")
                || (finish is not null && finish != "STOP" && finish != "MAX_TOKENS"))
                throw new AIProviderException(AIProviderFailure.InvalidResponse);
            var parts = Property(Property(candidate, "content"), "parts");
            var text = parts.ValueKind == JsonValueKind.Array
                ? string.Concat(parts.EnumerateArray()
                    .Where(part => Property(part, "thought").ValueKind != JsonValueKind.True)
                    .Select(part => Text(Property(part, "text")))) : null;
            var content = RequireText(text);
            var usage = Property(json, "usageMetadata");
            return new AIOptimizationResponse(content, "Optimized prompt using Google Gemini API.",
                new TokenUsage(Count(usage, "promptTokenCount"), Count(usage, "candidatesTokenCount")),
                stopwatch.Elapsed.TotalMilliseconds);
        }, cancellationToken);
    }
}
