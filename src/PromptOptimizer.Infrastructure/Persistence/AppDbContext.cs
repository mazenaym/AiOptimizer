using Microsoft.EntityFrameworkCore;
using PromptOptimizer.Domain.Entities;

namespace PromptOptimizer.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<RefreshToken> RefreshTokens
        => Set<RefreshToken>();

    public DbSet<Prompt> Prompts
        => Set<Prompt>();

    public DbSet<PromptCategory> PromptCategories
        => Set<PromptCategory>();

    public DbSet<Optimization> Optimizations
        => Set<Optimization>();

    public DbSet<AIProvider> AIProviders
        => Set<AIProvider>();

    public DbSet<AIModel> AIModels
        => Set<AIModel>();

    public DbSet<PromptTemplate> PromptTemplates
        => Set<PromptTemplate>();

    public DbSet<UsageRecord> UsageRecords
        => Set<UsageRecord>();

    public DbSet<Plan> Plans
        => Set<Plan>();

    public DbSet<Subscription> Subscriptions
        => Set<Subscription>();


    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);
    }
}
