using System.Text.Json.Serialization;

namespace PromptOptimizer.Api.Contracts;

public sealed record ApiErrorResponse(
    [property: JsonPropertyName("status")]
    int Status,

    [property: JsonPropertyName("message")]
    string Message,

    [property: JsonPropertyName("errors")]
    IReadOnlyDictionary<string, string[]>? Errors,

    [property: JsonPropertyName("traceId")]
    string TraceId);