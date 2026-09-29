using PromptOptimizer.Application.Common.Interfaces;
using PromptOptimizer.Application.Common.Exceptions;

namespace PromptOptimizer.Infrastructure.AI;

public class AIServiceFactory : IAIServiceFactory
{
    private readonly IEnumerable<IAIService> _aiServices;

    public AIServiceFactory(IEnumerable<IAIService> aiServices)
    {
        _aiServices = aiServices;
    }

    public IAIService GetService(string providerName)
    {
        var matches = _aiServices.Where(s => s.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase))
            .Take(2).ToArray();
        return matches.Length == 1 ? matches[0] : throw new AIProviderException(
            AIProviderFailure.Configuration, "The requested AI provider is unsupported or is not uniquely registered.");
    }
}
