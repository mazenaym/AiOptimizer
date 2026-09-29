using PromptOptimizer.Application.Common.Interfaces;
using PromptOptimizer.Application.Optimization.Models;

namespace PromptOptimizer.Application.Optimization.Services;

public interface IPromptOptimizerEngine
{
    Task<AIOptimizationResponse> OptimizeAsync(PromptOptimizationRequest request, CancellationToken cancellationToken = default);
}

public class PromptOptimizerEngine : IPromptOptimizerEngine
{
    private readonly IAIServiceFactory _aiServiceFactory;

    public PromptOptimizerEngine(IAIServiceFactory aiServiceFactory)
    {
        _aiServiceFactory = aiServiceFactory;
    }

    public async Task<AIOptimizationResponse> OptimizeAsync(PromptOptimizationRequest request, CancellationToken cancellationToken = default)
    {
        var service = _aiServiceFactory.GetService(request.ProviderCode);

        var systemInstructions = BuildSystemInstructions(request);

        return await service.OptimizePromptAsync(
            request.PromptContent,
            systemInstructions,
            request.ModelIdentifier,
            cancellationToken);
    }

    private static string BuildSystemInstructions(PromptOptimizationRequest request)
    {
        var category = string.IsNullOrWhiteSpace(request.CategoryName) ? "General" : request.CategoryName;
        var instructions = $"You are an expert Prompt Engineer. Optimize the user's prompt for high quality, clarity, and precision in the '{category}' domain.";
        if (!string.IsNullOrWhiteSpace(request.Language))
        {
            instructions += $" Use the prompt's language: {request.Language}.";
        }
        if (!string.IsNullOrWhiteSpace(request.CustomInstructions))
        {
            instructions += $" Additional instructions: {request.CustomInstructions}";
        }
        return instructions;
    }
}
