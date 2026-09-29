using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using PromptOptimizer.Application.Common.Interfaces;
using PromptOptimizer.Application.Optimization.Models;
using PromptOptimizer.Application.Optimization.Services;
using PromptOptimizer.Domain.ValueObjects;
using PromptOptimizer.Infrastructure.Persistence;

namespace PromptOptimizer.IntegrationTests;

internal sealed class OptimizationApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly string _signingKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
    public TestOptimizerEngine Engine { get; } = new();
    public string[] AllowedOrigins { get; init; } = [];

    public OptimizationApiFactory() => _connection.Open();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Production");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["Jwt:Key"] = _signingKey,
                ["Jwt:Issuer"] = "optimization-tests",
                ["Jwt:Audience"] = "optimization-tests",
                ["Jwt:AccessTokenExpirationMinutes"] = "10",
                ["Seed:Model:Enabled"] = "false"
            };
            for (var index = 0; index < AllowedOrigins.Length; index++)
                settings[$"Cors:AllowedOrigins:{index}"] = AllowedOrigins[index];
            configuration.AddInMemoryCollection(settings);
        });
        builder.ConfigureServices(services =>
        {
            // Minimal-host startup captures JWT settings before the factory's configuration
            // callback. Align the test credentials while keeping the real bearer handler,
            // validation flags, challenge response and current-user service intact.
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters.IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_signingKey));
                options.TokenValidationParameters.ValidIssuer = "optimization-tests";
                options.TokenValidationParameters.ValidAudience = "optimization-tests";
            });
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection)
                .ReplaceService<IMigrator, TestSchemaInitializer>());
            services.RemoveAll<IPromptOptimizerEngine>();
            services.AddSingleton<IPromptOptimizerEngine>(Engine);
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _connection.DisposeAsync();
    }

    // Production migrations are PostgreSQL-specific. Only this isolated test host uses
    // EnsureCreated for SQLite; the production startup and migration path stay unchanged.
    private sealed class TestSchemaInitializer(ICurrentDbContext currentContext) : IMigrator
    {
        public void Migrate(string? targetMigration = null) => currentContext.Context.Database.EnsureCreated();
        public async Task MigrateAsync(string? targetMigration = null, CancellationToken cancellationToken = default)
            => await currentContext.Context.Database.EnsureCreatedAsync(cancellationToken);
        public string GenerateScript(string? fromMigration = null, string? toMigration = null,
            MigrationsSqlGenerationOptions options = MigrationsSqlGenerationOptions.Default)
            => throw new NotSupportedException("This test host does not generate migration scripts.");
        public bool HasPendingModelChanges() => false;
    }
}

internal sealed class TestOptimizerEngine : IPromptOptimizerEngine
{
    private int _calls;
    public int Calls => _calls;
    public PromptOptimizationRequest? LastRequest { get; private set; }
    public Func<PromptOptimizationRequest, CancellationToken, Task<AIOptimizationResponse>> Respond { get; set; }
        = (_, _) => Task.FromResult(new AIOptimizationResponse("Optimized test output", "Not exposed", new TokenUsage(100, 25), 42));

    public Task<AIOptimizationResponse> OptimizeAsync(PromptOptimizationRequest request, CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _calls);
        LastRequest = request;
        return Respond(request, cancellationToken);
    }
}
