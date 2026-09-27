using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PromptOptimizer.Domain.Entities;

namespace PromptOptimizer.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        AppDbContext context,
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        var categories = new[]
        {
            (Name: "General", Code: "general"),
            (Name: "Coding", Code: "coding"),
            (Name: "Writing", Code: "writing"),
            (Name: "Research", Code: "research"),
            (Name: "Marketing", Code: "marketing"),
            (Name: "Analysis", Code: "analysis")
        };

        var existingCodes = await context.PromptCategories
            .Select(x => x.Code)
            .ToListAsync(cancellationToken);

        var existingCodeSet = existingCodes.ToHashSet(
            StringComparer.OrdinalIgnoreCase);

        foreach (var category in categories)
        {
            if (existingCodeSet.Contains(category.Code))
                continue;

            context.PromptCategories.Add(new PromptCategory
            {
                Name = category.Name,
                Code = category.Code,
                IsActive = true
            });
        }

        // Optional until a real provider/model has been selected.
        if (configuration.GetValue<bool>("Seed:Model:Enabled"))
        {
            var providerCode = Required(
                configuration,
                "Seed:Model:ProviderCode");

            var providerName = Required(
                configuration,
                "Seed:Model:ProviderName");

            var modelIdentifier = Required(
                configuration,
                "Seed:Model:Identifier");

            var modelName = Required(
                configuration,
                "Seed:Model:Name");

            var provider = await context.AIProviders
                .SingleOrDefaultAsync(
                    x => x.Code == providerCode,
                    cancellationToken);

            if (provider is null)
            {
                provider = new AIProvider
                {
                    Code = providerCode,
                    Name = providerName,
                    IsActive = true
                };

                context.AIProviders.Add(provider);
            }

            var existingModel = await context.AIModels
                .SingleOrDefaultAsync(
                    x => x.ModelIdentifier == modelIdentifier,
                    cancellationToken);

            if (existingModel is null)
            {
                context.AIModels.Add(new AIModel
                {
                    Provider = provider,
                    Name = modelName,
                    ModelIdentifier = modelIdentifier,
                    IsActive = true,

                    // Unknown pricing/capabilities stay unknown.
                    InputPricePerMillionTokens = null,
                    OutputPricePerMillionTokens = null,
                    ContextWindow = null
                });
            }
            else if (existingModel.ProviderId != provider.Id)
            {
                throw new InvalidOperationException(
                    "The seeded model already belongs to another provider.");
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static string Required(
        IConfiguration configuration,
        string key)
    {
        var value = configuration[key];

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Required seed setting '{key}' is missing.");
        }

        return value.Trim();
    }
}