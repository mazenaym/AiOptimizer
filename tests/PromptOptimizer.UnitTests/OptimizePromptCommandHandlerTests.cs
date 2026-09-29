using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PromptOptimizer.Application;
using PromptOptimizer.Application.Common.Exceptions;
using PromptOptimizer.Application.Common.Interfaces;
using PromptOptimizer.Application.Optimization.Models;
using PromptOptimizer.Application.Optimization.Services;
using PromptOptimizer.Application.Prompts.Commands.OptimizePrompt;
using PromptOptimizer.Domain.Entities;
using PromptOptimizer.Domain.ValueObjects;
using PromptOptimizer.Infrastructure.Persistence;

namespace PromptOptimizer.UnitTests;

public sealed class OptimizePromptCommandHandlerTests : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private CountingContext _context = null!;
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IPromptOptimizerEngine> _engine = new(MockBehavior.Strict);
    private readonly Guid _userId = Guid.NewGuid();
    private Prompt _prompt = null!;
    private AIModel _model = null!;
    private AIProvider _provider = null!;

    private static AIOptimizationResponse Response(TokenUsage? usage = null) =>
        new("Optimized content", "Canned explanation should not be returned", usage ?? new TokenUsage(1200, 300), 150.6);

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        _context = new CountingContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);
        await _context.Database.EnsureCreatedAsync();
        var user = new ApplicationUser
        {
            Id = _userId, UserName = "owner", Email = "owner@example.test", PasswordHash = "test-only-hash"
        };
        _provider = new AIProvider { Code = "gemini", Name = "Google Gemini", IsActive = true };
        _model = new AIModel
        {
            Provider = _provider, Name = "Selected model", ModelIdentifier = "selected-model",
            IsActive = true, InputPricePerMillionTokens = 2m, OutputPricePerMillionTokens = 6m
        };
        _prompt = new Prompt
        {
            User = user, OriginalContent = "  Original user content\n", Language = "Arabic",
            Category = new PromptCategory { Name = "Database-backed category", Code = "database-category" }
        };
        _context.Prompts.Add(_prompt);
        _context.AIModels.Add(_model);
        await _context.SaveChangesAsync();
        _context.SaveCalls = 0;
        _currentUser.SetupGet(x => x.UserId).Returns(_userId);
        _currentUser.SetupGet(x => x.IsAuthenticated).Returns(true);
        _engine.Setup(x => x.OptimizeAsync(It.IsAny<PromptOptimizationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response());
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _connection.DisposeAsync();
    }

    private OptimizePromptCommand Command() => new(_prompt.Id, _model.Id, "Use bullet points");
    private OptimizePromptCommandHandler Handler() => new(_context, _engine.Object, _currentUser.Object);

    [Fact]
    public async Task Owned_prompt_saves_linked_optimization_and_usage_once_and_returns_current_dto()
    {
        using var cancellation = new CancellationTokenSource();
        var result = await Handler().Handle(Command(), cancellation.Token);
        Assert.Equal(1, _context.SaveCalls);
        Assert.Equal(cancellation.Token, _context.LastSaveToken);
        _context.ChangeTracker.Clear();
        var optimization = await _context.Optimizations.SingleAsync();
        var usage = await _context.UsageRecords.SingleAsync();
        Assert.Equal(_prompt.Id, optimization.PromptId);
        Assert.Equal(_model.Id, optimization.ModelId);
        Assert.Equal("Optimized content", optimization.OptimizedContent);
        Assert.Equal(151, optimization.ProcessingTimeMs);
        Assert.Equal(_userId, usage.UserId);
        Assert.Equal(_model.Id, usage.ModelId);
        Assert.Equal(optimization.Id, usage.OptimizationId);
        Assert.Equal(1200, usage.InputTokens);
        Assert.Equal(300, usage.OutputTokens);
        Assert.Equal(1500, usage.TotalTokens);
        Assert.Equal(0.0042m, usage.EstimatedCost);
        Assert.Equal(optimization.Id, result.OptimizationId);
        Assert.Equal(_prompt.OriginalContent, result.OriginalContent);
        Assert.Equal(_model.ModelIdentifier, result.ModelIdentifier);
        Assert.Equal(_provider.Code, result.ProviderCode);
        Assert.Equal(usage.EstimatedCost, result.EstimatedCost);
        Assert.Equal(optimization.CreatedAt, result.CreatedAt);
        Assert.Equal(_prompt.OriginalContent, (await _context.Prompts.SingleAsync()).OriginalContent);
        Assert.Equal(1, await _context.AIModels.CountAsync());
        Assert.Equal(1, await _context.Prompts.CountAsync());
        _engine.Verify(x => x.OptimizeAsync(It.Is<PromptOptimizationRequest>(r =>
            r.PromptContent == _prompt.OriginalContent
            && r.CategoryName == "Database-backed category" && r.Language == "Arabic"
            && r.ProviderCode == _provider.Code && r.ModelIdentifier == _model.ModelIdentifier
            && r.CustomInstructions == "Use bullet points"), cancellation.Token), Times.Once);
    }

    [Fact]
    public async Task Provider_usage_does_not_become_prompt_size_or_savings()
    {
        var result = await Handler().Handle(Command(), default);
        _context.ChangeTracker.Clear();
        var optimization = await _context.Optimizations.SingleAsync();
        Assert.Null(optimization.OriginalTokens);
        Assert.Null(optimization.OptimizedTokens);
        Assert.Null(optimization.TokensSaved);
        Assert.Null(optimization.ReductionPercentage);
        Assert.Null(optimization.EstimatedOriginalCost);
        Assert.Null(optimization.EstimatedOptimizedCost);
        Assert.Null(optimization.EstimatedCostSaved);
        Assert.Null(result.OriginalTokens);
        Assert.Null(result.OptimizedTokens);
        Assert.Null(result.TokensSaved);
        Assert.Equal(1200, result.ProviderInputTokens);
        Assert.Equal(300, result.ProviderOutputTokens);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Missing_or_foreign_prompt_is_not_found_before_engine_call(bool foreign)
    {
        if (foreign) _currentUser.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
        var command = foreign ? Command() : Command() with { PromptId = Guid.NewGuid() };
        await Assert.ThrowsAsync<NotFoundException>(() => Handler().Handle(command, default));
        await AssertNoInvocationOrWrites();
    }

    [Fact]
    public async Task Unauthenticated_user_is_rejected_before_engine_call()
    {
        _currentUser.SetupGet(x => x.IsAuthenticated).Returns(false);
        await Assert.ThrowsAsync<UnauthorizedException>(() => Handler().Handle(Command(), default));
        await AssertNoInvocationOrWrites();
    }

    [Fact]
    public async Task Missing_model_is_not_found_and_never_created()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => Handler().Handle(Command() with { ModelId = Guid.NewGuid() }, default));
        Assert.Equal(1, await _context.AIModels.CountAsync());
        await AssertNoInvocationOrWrites();
    }

    [Theory]
    [InlineData("inactive-model")]
    [InlineData("inactive-provider")]
    [InlineData("missing-identifier")]
    [InlineData("missing-code")]
    [InlineData("negative-price")]
    public async Task Invalid_catalog_metadata_is_rejected_before_engine_call(string reason)
    {
        switch (reason)
        {
            case "inactive-model": _model.IsActive = false; break;
            case "inactive-provider": _provider.IsActive = false; break;
            case "missing-identifier": _model.ModelIdentifier = " "; break;
            case "missing-code": _provider.Code = " "; break;
            case "negative-price": _model.InputPricePerMillionTokens = -1; break;
        }
        await _context.SaveChangesAsync();
        _context.SaveCalls = 0;
        await Assert.ThrowsAsync<ValidationException>(() => Handler().Handle(Command(), default));
        await AssertNoInvocationOrWrites();
    }

    [Fact]
    public async Task Missing_provider_is_rejected_before_engine_call()
    {
        // Deliberately simulate a broken catalog without changing production FK rules.
        await _context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = OFF");
        _model.ProviderId = Guid.NewGuid();
        await _context.SaveChangesAsync();
        await _context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON");
        _context.ChangeTracker.Clear();
        _context.SaveCalls = 0;
        await Assert.ThrowsAsync<ValidationException>(() => Handler().Handle(Command(), default));
        await AssertNoInvocationOrWrites();
    }

    [Fact]
    public async Task No_category_or_language_is_passed_cleanly_to_engine()
    {
        _prompt.Category = null;
        _prompt.CategoryId = null;
        _prompt.Language = null;
        await _context.SaveChangesAsync();
        await Handler().Handle(Command() with { CustomInstructions = null }, default);
        _engine.Verify(x => x.OptimizeAsync(It.Is<PromptOptimizationRequest>(r =>
            r.CategoryName == null && r.Language == null && r.CustomInstructions == null), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(AIProviderFailure.Configuration)]
    [InlineData(AIProviderFailure.Authentication)]
    [InlineData(AIProviderFailure.RateLimit)]
    [InlineData(AIProviderFailure.Unavailable)]
    [InlineData(AIProviderFailure.InvalidResponse)]
    public async Task Provider_failure_propagates_without_success_records(AIProviderFailure failure)
    {
        var expected = new AIProviderException(failure);
        _engine.Setup(x => x.OptimizeAsync(It.IsAny<PromptOptimizationRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expected);
        var actual = await Assert.ThrowsAsync<AIProviderException>(() => Handler().Handle(Command(), default));
        Assert.Same(expected, actual);
        await AssertNoWrites();
    }

    [Fact]
    public async Task Cancellation_before_call_does_not_invoke_engine()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Handler().Handle(Command(), cancellation.Token));
        await AssertNoInvocationOrWrites();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Cancellation_during_provider_call_propagates_without_writes(bool engineReturns)
    {
        using var cancellation = new CancellationTokenSource();
        _engine.Setup(x => x.OptimizeAsync(It.IsAny<PromptOptimizationRequest>(), cancellation.Token))
            .Returns(() =>
            {
                cancellation.Cancel();
                return engineReturns ? Task.FromResult(Response()) : Task.FromCanceled<AIOptimizationResponse>(cancellation.Token);
            });
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Handler().Handle(Command(), cancellation.Token));
        await AssertNoWrites();
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(1200, null)]
    [InlineData(null, 300)]
    public async Task Unknown_usage_and_cost_remain_unknown_in_database_and_result(int? input, int? output)
    {
        _engine.Setup(x => x.OptimizeAsync(It.IsAny<PromptOptimizationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response(new TokenUsage(input, output)));
        var result = await Handler().Handle(Command(), default);
        _context.ChangeTracker.Clear();
        var usage = await _context.UsageRecords.SingleAsync();
        Assert.Equal(input, usage.InputTokens);
        Assert.Equal(output, usage.OutputTokens);
        Assert.Null(usage.TotalTokens);
        Assert.Null(usage.EstimatedCost);
        Assert.Null(result.EstimatedCost);
        Assert.Null(result.ProviderTotalTokens);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Missing_prices_are_unknown_even_for_model_marked_free(bool missingInput, bool missingOutput)
    {
        if (missingInput) _model.InputPricePerMillionTokens = null;
        if (missingOutput) _model.OutputPricePerMillionTokens = null;
        _model.IsFree = true;
        await _context.SaveChangesAsync();
        var result = await Handler().Handle(Command(), default);
        _context.ChangeTracker.Clear();
        Assert.Null((await _context.UsageRecords.SingleAsync()).EstimatedCost);
        Assert.Null(result.EstimatedCost);
        Assert.Equal(1500, result.ProviderTotalTokens);
    }

    [Fact]
    public async Task Explicit_zero_prices_produce_known_zero_cost()
    {
        _model.InputPricePerMillionTokens = 0;
        _model.OutputPricePerMillionTokens = 0;
        await _context.SaveChangesAsync();
        var result = await Handler().Handle(Command(), default);
        Assert.Equal(0m, result.EstimatedCost);
    }

    [Fact]
    public async Task Failed_usage_insert_rolls_back_optimization_insert()
    {
        await _context.Database.ExecuteSqlRawAsync("""
            CREATE TRIGGER reject_usage BEFORE INSERT ON usage_records
            BEGIN SELECT RAISE(ABORT, 'Simulated usage write failure'); END;
            """);
        await Assert.ThrowsAsync<DbUpdateException>(() => Handler().Handle(Command(), default));
        Assert.Equal(1, _context.SaveCalls);
        _context.ChangeTracker.Clear();
        Assert.Empty(await _context.Optimizations.ToListAsync());
        Assert.Empty(await _context.UsageRecords.ToListAsync());
    }

    [Fact]
    public async Task MediatR_discovers_handler_and_enforces_request_validation()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplicationServices();
        services.AddSingleton<IAppDbContext>(_context);
        services.AddSingleton(_currentUser.Object);
        services.AddSingleton(_engine.Object);
        using var container = services.BuildServiceProvider();
        var sender = container.GetRequiredService<ISender>();
        await Assert.ThrowsAsync<ValidationException>(() => sender.Send(new OptimizePromptCommand(Guid.Empty, Guid.Empty)));
        await AssertNoInvocationOrWrites();
        var result = await sender.Send(Command());
        Assert.Equal(_prompt.Id, result.PromptId);
    }

    [Fact]
    public void Validator_limits_instructions_and_requires_identifiers()
    {
        var validator = new OptimizePromptCommandValidator();
        Assert.True(validator.Validate(Command() with { CustomInstructions = null }).IsValid);
        Assert.True(validator.Validate(Command() with { CustomInstructions = new string('x', 10_000) }).IsValid);
        Assert.False(validator.Validate(Command() with { CustomInstructions = new string('x', 10_001) }).IsValid);
        Assert.False(validator.Validate(Command() with { PromptId = Guid.Empty }).IsValid);
        Assert.False(validator.Validate(Command() with { ModelId = Guid.Empty }).IsValid);
    }

    private async Task AssertNoInvocationOrWrites()
    {
        _engine.Verify(x => x.OptimizeAsync(It.IsAny<PromptOptimizationRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        await AssertNoWrites();
    }

    private async Task AssertNoWrites()
    {
        Assert.Equal(0, _context.SaveCalls);
        Assert.Empty(await _context.Optimizations.ToListAsync());
        Assert.Empty(await _context.UsageRecords.ToListAsync());
        Assert.DoesNotContain(_context.ChangeTracker.Entries(), x => x.State == EntityState.Added);
    }

    private sealed class CountingContext(DbContextOptions<AppDbContext> options) : AppDbContext(options)
    {
        public int SaveCalls { get; set; }
        public CancellationToken LastSaveToken { get; private set; }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCalls++;
            LastSaveToken = cancellationToken;
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
