using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PromptOptimizer.Api.Contracts;
using PromptOptimizer.Application.Auth.Interfaces;
using PromptOptimizer.Application.Common.Exceptions;
using PromptOptimizer.Application.Common.Interfaces;
using PromptOptimizer.Application.Optimization.DTOs;
using PromptOptimizer.Domain.Entities;
using PromptOptimizer.Domain.ValueObjects;
using PromptOptimizer.Infrastructure.Persistence;

namespace PromptOptimizer.IntegrationTests;

public sealed class OptimizePromptEndpointTests : IAsyncLifetime
{
    private readonly OptimizationApiFactory _factory = new();
    private HttpClient _client = null!;
    private readonly Guid _ownerId = Guid.NewGuid();
    private readonly Guid _otherUserId = Guid.NewGuid();
    private Guid _promptId;
    private Guid _modelId;
    private Guid _providerId;
    private string Route => $"/api/prompts/{_promptId}/optimize";

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false
        });
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var owner = new ApplicationUser { Id = _ownerId, UserName = "owner", Email = "owner@example.test", PasswordHash = "test-only" };
        context.Users.Add(new ApplicationUser { Id = _otherUserId, UserName = "other", Email = "other@example.test", PasswordHash = "test-only" });
        var provider = new AIProvider { Name = "Gemini", Code = "gemini", IsActive = true };
        var model = new AIModel
        {
            Name = "Test model", ModelIdentifier = "trusted-model", Provider = provider, IsActive = true,
            InputPricePerMillionTokens = 2m, OutputPricePerMillionTokens = 4m
        };
        var prompt = new Prompt { User = owner, OriginalContent = "Persisted original", Language = "English" };
        context.Prompts.Add(prompt);
        context.AIModels.Add(model);
        await context.SaveChangesAsync();
        _promptId = prompt.Id;
        _modelId = model.Id;
        _providerId = provider.Id;
        Authenticate(_ownerId);
    }

    private void Authenticate(Guid id)
    {
        using var scope = _factory.Services.CreateScope();
        var token = scope.ServiceProvider.GetRequiredService<IJwtService>().GenerateAccessToken(
            new ApplicationUser { Id = id, Email = "test@example.test" });
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Missing_or_invalid_bearer_token_is_rejected(bool invalid)
    {
        _client.DefaultRequestHeaders.Authorization = invalid ? new AuthenticationHeaderValue("Bearer", "invalid-token") : null;
        using var response = await _client.PostAsJsonAsync(Route, new OptimizePromptRequest(_modelId));
        await AssertError(response, HttpStatusCode.Unauthorized);
        Assert.Contains(response.Headers.WwwAuthenticate, x => x.Scheme == "Bearer");
        await AssertNoWritesOrEngine();
    }

    [Fact]
    public async Task Owner_can_optimize_and_response_matches_persisted_records()
    {
        using var response = await _client.PostAsJsonAsync(Route, new OptimizePromptRequest(_modelId, "Be concise"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = (await response.Content.ReadFromJsonAsync<OptimizationResultDto>())!;
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var optimization = await db.Optimizations.SingleAsync();
        var usage = await db.UsageRecords.SingleAsync();
        Assert.Equal(optimization.Id, result.OptimizationId);
        Assert.Equal(_promptId, result.PromptId);
        Assert.Equal(_modelId, result.ModelId);
        Assert.Equal("Persisted original", result.OriginalContent);
        Assert.Equal(optimization.OptimizedContent, result.OptimizedContent);
        Assert.Equal("gemini", result.ProviderCode);
        Assert.Equal("trusted-model", result.ModelIdentifier);
        Assert.Equal(42, result.ProcessingTimeMs);
        Assert.Equal(optimization.Id, usage.OptimizationId);
        Assert.Equal(_ownerId, usage.UserId);
        Assert.Equal(_modelId, usage.ModelId);
        Assert.Equal(100, result.ProviderInputTokens);
        Assert.Equal(25, result.ProviderOutputTokens);
        Assert.Equal(125, result.ProviderTotalTokens);
        Assert.Equal(0.0003m, result.EstimatedCost);
        Assert.Equal(usage.EstimatedCost, result.EstimatedCost);
        Assert.Null(result.OriginalTokens);
        Assert.Null(result.OptimizedTokens);
        Assert.Null(result.TokensSaved);
        Assert.Equal(1, _factory.Engine.Calls);
        Assert.Equal("Be concise", _factory.Engine.LastRequest!.CustomInstructions);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.False(body.RootElement.TryGetProperty("status", out _));
        Assert.False(body.RootElement.TryGetProperty("explanation", out _));
        Assert.False(body.RootElement.TryGetProperty("systemInstructions", out _));
    }

    [Fact]
    public async Task Extra_body_fields_cannot_override_trusted_route_or_database_values()
    {
        using var response = await _client.PostAsJsonAsync(Route, new
        {
            modelId = _modelId, promptId = Guid.NewGuid(), userId = _otherUserId,
            providerCode = "evil", modelIdentifier = "evil", promptContent = "evil"
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Persisted original", _factory.Engine.LastRequest!.PromptContent);
        Assert.Equal("gemini", _factory.Engine.LastRequest.ProviderCode);
        Assert.Equal("trusted-model", _factory.Engine.LastRequest.ModelIdentifier);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(_ownerId, (await db.UsageRecords.SingleAsync()).UserId);
        Assert.Equal(_promptId, (await db.Optimizations.SingleAsync()).PromptId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Missing_or_foreign_prompt_returns_not_found_before_ai(bool foreign)
    {
        if (foreign) Authenticate(_otherUserId);
        var route = foreign ? Route : $"/api/prompts/{Guid.NewGuid()}/optimize";
        using var response = await _client.PostAsJsonAsync(route, new OptimizePromptRequest(_modelId));
        await AssertError(response, HttpStatusCode.NotFound);
        await AssertNoWritesOrEngine();
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("empty")]
    [InlineData("instructions")]
    public async Task Command_validation_returns_standard_bad_request(string invalid)
    {
        object request = invalid switch
        {
            "missing" => new { customInstructions = "hi" },
            "empty" => new OptimizePromptRequest(Guid.Empty),
            _ => new OptimizePromptRequest(_modelId, new string('x', 10_001))
        };
        using var response = await _client.PostAsJsonAsync(Route, request);
        var error = await AssertError(response, HttpStatusCode.BadRequest);
        Assert.Contains(invalid == "instructions" ? "CustomInstructions" : "ModelId", error.Errors!.Keys);
        await AssertNoWritesOrEngine();
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("inactive-model")]
    [InlineData("inactive-provider")]
    public async Task Invalid_catalog_is_rejected(string problem)
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            if (problem == "inactive-model") (await db.AIModels.SingleAsync()).IsActive = false;
            if (problem == "inactive-provider") (await db.AIProviders.SingleAsync(x => x.Id == _providerId)).IsActive = false;
            await db.SaveChangesAsync();
        }
        using var response = await _client.PostAsJsonAsync(Route,
            new OptimizePromptRequest(problem == "missing" ? Guid.NewGuid() : _modelId));
        await AssertError(response, problem == "missing" ? HttpStatusCode.NotFound : HttpStatusCode.BadRequest);
        await AssertNoWritesOrEngine();
    }

    [Theory]
    [InlineData(AIProviderFailure.RateLimit, HttpStatusCode.TooManyRequests)]
    [InlineData(AIProviderFailure.Unavailable, HttpStatusCode.ServiceUnavailable)]
    [InlineData(AIProviderFailure.InvalidResponse, HttpStatusCode.BadGateway)]
    [InlineData(AIProviderFailure.Authentication, HttpStatusCode.ServiceUnavailable)]
    [InlineData(AIProviderFailure.Configuration, HttpStatusCode.ServiceUnavailable)]
    public async Task Provider_failures_are_mapped_without_leaking_details(AIProviderFailure failure, HttpStatusCode status)
    {
        _factory.Engine.Respond = (_, _) => throw new AIProviderException(failure,
            "API_KEY=secret Authorization: Bearer secret https://provider.test/?key=secret raw-upstream-body stack-trace");
        using var response = await _client.PostAsJsonAsync(Route, new OptimizePromptRequest(_modelId));
        await AssertError(response, status);
        Assert.Empty(response.Headers.WwwAuthenticate);
        var payload = await response.Content.ReadAsStringAsync();
        foreach (var secret in new[] { "secret", "Authorization", "provider.test", "raw-upstream-body", "stack-trace", "AIProviderException" })
            Assert.DoesNotContain(secret, payload);
        Assert.Equal(1, _factory.Engine.Calls);
        await AssertNoWrites();
    }

    [Fact]
    public async Task Unknown_usage_and_cost_remain_null_in_json_and_database()
    {
        _factory.Engine.Respond = (_, _) => Task.FromResult(new AIOptimizationResponse("Output", "Not exposed", TokenUsage.Unknown, 1));
        using var response = await _client.PostAsJsonAsync(Route, new OptimizePromptRequest(_modelId));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        foreach (var field in new[] { "originalTokens", "optimizedTokens", "tokensSaved", "providerInputTokens", "providerOutputTokens", "providerTotalTokens", "estimatedCost" })
            Assert.Equal(JsonValueKind.Null, body.RootElement.GetProperty(field).ValueKind);
        using var scope = _factory.Services.CreateScope();
        var usage = await scope.ServiceProvider.GetRequiredService<AppDbContext>().UsageRecords.SingleAsync();
        Assert.Null(usage.InputTokens);
        Assert.Null(usage.EstimatedCost);
    }

    [Fact]
    public async Task Client_cancellation_reaches_engine_and_does_not_persist_records()
    {
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var cancelled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _factory.Engine.Respond = async (_, token) =>
        {
            started.SetResult();
            try { await Task.Delay(Timeout.InfiniteTimeSpan, token); }
            catch (OperationCanceledException) { cancelled.SetResult(); throw; }
            throw new InvalidOperationException("Unreachable");
        };
        using var cancellation = new CancellationTokenSource();
        var pending = _client.PostAsJsonAsync(Route, new OptimizePromptRequest(_modelId), cancellation.Token);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(10));
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await pending);
        await cancelled.Task.WaitAsync(TimeSpan.FromSeconds(10));
        await AssertNoWrites();
    }

    private static async Task<ApiErrorResponse> AssertError(HttpResponseMessage response, HttpStatusCode status)
    {
        Assert.Equal(status, response.StatusCode);
        var error = (await response.Content.ReadFromJsonAsync<ApiErrorResponse>())!;
        Assert.Equal((int)status, error.Status);
        Assert.False(string.IsNullOrWhiteSpace(error.Message));
        Assert.False(string.IsNullOrWhiteSpace(error.TraceId));
        return error;
    }

    private async Task AssertNoWritesOrEngine()
    {
        Assert.Equal(0, _factory.Engine.Calls);
        await AssertNoWrites();
    }

    private async Task AssertNoWrites()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Empty(await db.Optimizations.ToListAsync());
        Assert.Empty(await db.UsageRecords.ToListAsync());
    }
}
