namespace PromptOptimizer.Domain.ValueObjects;

public record TokenUsage(int PromptTokens, int CompletionTokens)
{
    public int TotalTokens => PromptTokens + CompletionTokens;

    public static TokenUsage Empty => new(0, 0);

    public TokenUsage Add(TokenUsage other)
    {
        return new TokenUsage(PromptTokens + other.PromptTokens, CompletionTokens + other.CompletionTokens);
    }
}
