using Microsoft.EntityFrameworkCore;
using PromptOptimizer.Domain.Entities;
using DomainOptimization =
    PromptOptimizer.Domain.Entities.Optimization;


namespace PromptOptimizer.Application.Common.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<AIModel> AIModels { get; }
        DbSet<AIProvider> AIProviders { get; }
        DbSet<DomainOptimization> Optimizations { get; }
        DbSet<Plan> Plans { get; }
        DbSet<PromptCategory> PromptCategories { get; }
        DbSet<Prompt> Prompts { get; }
        DbSet<PromptTemplate> PromptTemplates { get; }
        DbSet<RefreshToken> RefreshTokens { get; }
        DbSet<Subscription> Subscriptions { get; }
        DbSet<UsageRecord> UsageRecords { get; }
        DbSet<ApplicationUser> Users { get; }
    }
}