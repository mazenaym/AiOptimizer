using PromptOptimizer.Application.Common.Interfaces;

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
        var service = _aiServices.FirstOrDefault(s => s.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));
        return service ?? _aiServices.First();
    }
}
