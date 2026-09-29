using Microsoft.Extensions.DependencyInjection;
using PromptOptimizer.Application.Common.Interfaces;
using PromptOptimizer.Infrastructure.AI.Gemini;
using PromptOptimizer.Infrastructure.AI.OpenRouter;
using PromptOptimizer.Infrastructure.AI.Ollama;

namespace PromptOptimizer.Infrastructure.ExternalServices;

public static class ExternalServiceExtensions
{
    public static IServiceCollection AddExternalServices(this IServiceCollection services)
    {
        services.AddHttpClient<OpenRouterService>().RedactLoggedHeaders(_ => true);
        services.AddHttpClient<GeminiService>().RedactLoggedHeaders(_ => true);
        services.AddHttpClient<OllamaService>().RedactLoggedHeaders(_ => true);
        services.AddTransient<IAIService>(sp => sp.GetRequiredService<OpenRouterService>());
        services.AddTransient<IAIService>(sp => sp.GetRequiredService<GeminiService>());
        services.AddTransient<IAIService>(sp => sp.GetRequiredService<OllamaService>());
        return services;
    }
}
