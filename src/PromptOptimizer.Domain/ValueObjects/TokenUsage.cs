namespace PromptOptimizer.Domain.ValueObjects;

// Raw provider usage, not counts of the original and optimized prompt text.
// Null means the provider did not report that count; zero is a known count.
public record TokenUsage(int? PromptTokens, int? CompletionTokens)
{
    public int? TotalTokens => PromptTokens + CompletionTokens;

    public static TokenUsage Unknown => new(null, null);

    public static TokenUsage Empty => new(0, 0);

    public TokenUsage Add(TokenUsage other)
    {
        return new TokenUsage(PromptTokens + other.PromptTokens, CompletionTokens + other.CompletionTokens);
    }
}
