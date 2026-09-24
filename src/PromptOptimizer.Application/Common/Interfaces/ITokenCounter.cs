namespace PromptOptimizer.Application.Common.Interfaces;

public interface ITokenCounter
{
    int CountTokens(string text, string? modelName = null);
}
