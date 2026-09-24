using PromptOptimizer.Domain.Enums;

namespace PromptOptimizer.Application.Optimization.Models;

public record PromptOptimizationRequest(
    string PromptContent,
    PromptCategory Category,
    string TargetModelProvider, // OpenRouter, Gemini, Ollama
    string TargetModelKey,
    string? CustomInstructions = null
);
