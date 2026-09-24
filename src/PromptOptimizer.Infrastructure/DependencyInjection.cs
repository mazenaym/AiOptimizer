using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PromptOptimizer.Application.Common.Interfaces;
using PromptOptimizer.Infrastructure.AI;
using PromptOptimizer.Infrastructure.AI.Gemini;
using PromptOptimizer.Infrastructure.AI.Ollama;
using PromptOptimizer.Infrastructure.AI.OpenRouter;
//using PromptOptimizer.Infrastructure.Authentication;
//using PromptOptimizer.Infrastructure.Caching;
using PromptOptimizer.Infrastructure.ExternalServices;
using PromptOptimizer.Infrastructure.Persistence;
//using PromptOptimizer.Infrastructure.Tokenization;

namespace PromptOptimizer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? "Data Source=PromptOptimizer.db";

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IAppDbContext>(provider => (IAppDbContext)provider.GetRequiredService<AppDbContext>());

        // Auth & Security
        //services.AddSingleton<IPasswordHasher, PasswordHasher>();
        //services.AddSingleton<IJwtTokenGenerator, JwtService>();

        // Caching
        services.AddDistributedMemoryCache(); // Fallback for Redis or memory cache
        //services.AddSingleton<ICacheService, RedisCacheService>();

        // Tokenization
        //services.AddSingleton<ITokenCounter, TokenCounter>();
        //services.AddSingleton<TokenizerFactory>();

        // AI Services & HTTP
        services.AddExternalServices();
        //services.AddHttpClient<OpenRouterService>();
        //services.AddHttpClient<GeminiService>();
        //services.AddHttpClient<OllamaService>();

        //services.AddTransient<IAIService>(sp => sp.GetRequiredService<OpenRouterService>());
        //services.AddTransient<IAIService>(sp => sp.GetRequiredService<GeminiService>());
        //services.AddTransient<IAIService>(sp => sp.GetRequiredService<OllamaService>());
        services.AddScoped<IAIServiceFactory, AIServiceFactory>();

        return services;
    }
}
