using System.Net;
using System.Text.Json;
using PromptOptimizer.Application.Common.Exceptions;

namespace PromptOptimizer.Infrastructure.AI;

internal static class ProviderHttp
{
    // Reject credentials in URLs before HttpClient's diagnostic logging can observe them.
    public static Uri Endpoint(string baseUrl, string path)
    {
        if (!Uri.TryCreate(baseUrl.TrimEnd('/') + "/", UriKind.Absolute, out var uri)
            || (uri.Scheme != "https" && uri.Scheme != "http")
            || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment)
            || !string.IsNullOrEmpty(uri.UserInfo))
            throw new AIProviderException(AIProviderFailure.Configuration);
        return new Uri(uri, path);
    }

    public static void RequireKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Any(char.IsControl))
            throw new AIProviderException(AIProviderFailure.Configuration);
    }

    public static async Task<T> SendAsync<T>(HttpClient client, HttpRequestMessage request,
        Func<JsonElement, T> parse, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            using var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var failure = response.StatusCode switch
                {
                    HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => AIProviderFailure.Authentication,
                    HttpStatusCode.TooManyRequests => AIProviderFailure.RateLimit,
                    HttpStatusCode.RequestTimeout => AIProviderFailure.Unavailable,
                    _ when (int)response.StatusCode >= 500 => AIProviderFailure.Unavailable,
                    _ => AIProviderFailure.InvalidResponse
                };
                throw new AIProviderException(failure);
            }
            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var result = parse(json.RootElement);
            cancellationToken.ThrowIfCancellationRequested();
            return result;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new AIProviderException(AIProviderFailure.Unavailable);
        }
        catch (HttpRequestException)
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw new AIProviderException(AIProviderFailure.Unavailable);
        }
        catch (IOException)
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw new AIProviderException(AIProviderFailure.Unavailable);
        }
        catch (JsonException)
        {
            throw new AIProviderException(AIProviderFailure.InvalidResponse);
        }
        catch (InvalidOperationException)
        {
            throw new AIProviderException(AIProviderFailure.InvalidResponse);
        }
    }

    public static JsonElement Property(JsonElement element, string name) =>
        element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value)
            ? value : default;

    public static JsonElement First(JsonElement element) =>
        element.ValueKind == JsonValueKind.Array && element.GetArrayLength() > 0
            ? element[0] : default;

    public static string? Text(JsonElement element) =>
        element.ValueKind == JsonValueKind.String ? element.GetString() : null;

    public static string RequireText(string? text) => !string.IsNullOrWhiteSpace(text)
        ? text : throw new AIProviderException(AIProviderFailure.InvalidResponse);

    public static int? Count(JsonElement element, string name)
    {
        if (element.ValueKind is not (JsonValueKind.Object or JsonValueKind.Undefined or JsonValueKind.Null))
            throw new AIProviderException(AIProviderFailure.InvalidResponse);
        var value = Property(element, name);
        if (value.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null) return null;
        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var count) && count >= 0)
            return count;
        throw new AIProviderException(AIProviderFailure.InvalidResponse);
    }
}
