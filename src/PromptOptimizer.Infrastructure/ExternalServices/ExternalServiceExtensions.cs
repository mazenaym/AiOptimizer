using Microsoft.Extensions.DependencyInjection;

namespace PromptOptimizer.Infrastructure.ExternalServices;

public static class ExternalServiceExtensions
{
    public static IServiceCollection AddExternalServices(this IServiceCollection services)
    {
        services.AddHttpClient();
        return services;
    }
}
