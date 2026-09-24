using Microsoft.EntityFrameworkCore;
using PromptOptimizer.Domain.Entities;

namespace PromptOptimizer.Infrastructure.Persistence
{
    public interface IAppDbContext
    {
        DbSet<AIModel> AIModels { get; }
        DbSet<AIProvider> AIProviders { get; }
        DbSet<Optimization> Optimizations { get; }
        DbSet<Plan> Plans { get; }
        DbSet<PromptCategory> PromptCategories { get; }
        DbSet<Prompt> Prompts { get; }
        DbSet<PromptTemplate> PromptTemplates { get; }
        DbSet<RefreshToken> RefreshTokens { get; }
        DbSet<Subscription> Subscriptions { get; }
        DbSet<UsageRecord> UsageRecords { get; }
        DbSet<User> Users { get; }
    }
}