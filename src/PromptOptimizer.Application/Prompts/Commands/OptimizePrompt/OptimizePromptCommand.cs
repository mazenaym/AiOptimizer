using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PromptOptimizer.Application.Common.Exceptions;
using PromptOptimizer.Application.Common.Extensions;
using PromptOptimizer.Application.Common.Interfaces;
using PromptOptimizer.Application.Optimization.DTOs;
using PromptOptimizer.Application.Optimization.Models;
using PromptOptimizer.Application.Optimization.Services;
using PromptOptimizer.Domain.Entities;
using AppValidationException = PromptOptimizer.Application.Common.Exceptions.ValidationException;
using DomainOptimization = PromptOptimizer.Domain.Entities.Optimization;

namespace PromptOptimizer.Application.Prompts.Commands.OptimizePrompt;

public sealed record OptimizePromptCommand(
    Guid PromptId,
    Guid ModelId,
    string? CustomInstructions = null)
    : IRequest<OptimizationResultDto>;

public sealed class OptimizePromptCommandValidator : AbstractValidator<OptimizePromptCommand>
{
    public OptimizePromptCommandValidator()
    {
        RuleFor(x => x.PromptId).NotEmpty();
        RuleFor(x => x.ModelId).NotEmpty();
        RuleFor(x => x.CustomInstructions).MaximumLength(10_000);
    }
}

public sealed class OptimizePromptCommandHandler
    : IRequestHandler<OptimizePromptCommand, OptimizationResultDto>
{
    private readonly IAppDbContext _context;
    private readonly IPromptOptimizerEngine _optimizerEngine;
    private readonly ICurrentUserService _currentUser;

    public OptimizePromptCommandHandler(
        IAppDbContext context,
        IPromptOptimizerEngine optimizerEngine,
        ICurrentUserService currentUser)
    {
        _context = context;
        _optimizerEngine = optimizerEngine;
        _currentUser = currentUser;
    }

    public async Task<OptimizationResultDto> Handle(
        OptimizePromptCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetRequiredUserId();
        cancellationToken.ThrowIfCancellationRequested();

        var prompt = await _context.Prompts
            .Include(x => x.Category)
            .SingleOrDefaultAsync(x => x.Id == request.PromptId && x.UserId == userId, cancellationToken)
            ?? throw new NotFoundException(nameof(Prompt), request.PromptId);

        var model = await _context.AIModels
            .SingleOrDefaultAsync(x => x.Id == request.ModelId, cancellationToken)
            ?? throw new NotFoundException(nameof(AIModel), request.ModelId);

        // Query explicitly so missing provider metadata is rejected before invoking the engine.
        var provider = await _context.AIProviders
            .SingleOrDefaultAsync(x => x.Id == model.ProviderId, cancellationToken);

        if (!model.IsActive || provider is null || !provider.IsActive
            || string.IsNullOrWhiteSpace(model.ModelIdentifier)
            || string.IsNullOrWhiteSpace(provider.Code))
        {
            throw new AppValidationException(new[]
            {
                new ValidationFailure(nameof(request.ModelId),
                    "The selected model or its provider is inactive or has incomplete configuration.")
            });
        }

        if (model.InputPricePerMillionTokens < 0 || model.OutputPricePerMillionTokens < 0)
        {
            throw new AppValidationException(new[]
            {
                new ValidationFailure(nameof(request.ModelId), "The selected model has invalid pricing.")
            });
        }

        var response = await _optimizerEngine.OptimizeAsync(
            new PromptOptimizationRequest(
                prompt.OriginalContent,
                prompt.Category?.Name,
                prompt.Language,
                provider.Code,
                model.ModelIdentifier,
                request.CustomInstructions),
            cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(response.OptimizedContent))
            throw new AIProviderException(AIProviderFailure.InvalidResponse);

        var optimization = new DomainOptimization
        {
            PromptId = prompt.Id,
            Prompt = prompt,
            ModelId = model.Id,
            Model = model,
            OptimizedContent = response.OptimizedContent,
            ProcessingTimeMs = double.IsFinite(response.ExecutionTimeMs)
                && response.ExecutionTimeMs >= 0 && response.ExecutionTimeMs <= int.MaxValue
                ? (int)Math.Round(response.ExecutionTimeMs, MidpointRounding.AwayFromZero)
                : null
            // Prompt-size estimates and savings remain unknown. Provider usage is not a substitute.
        };

        decimal? estimatedCost = null;
        if (response.TokenUsage.PromptTokens is int inputTokens
            && response.TokenUsage.CompletionTokens is int outputTokens
            && model.InputPricePerMillionTokens is decimal inputPrice
            && model.OutputPricePerMillionTokens is decimal outputPrice)
        {
            estimatedCost = Math.Round(
                inputTokens * inputPrice / 1_000_000m + outputTokens * outputPrice / 1_000_000m,
                10, MidpointRounding.AwayFromZero);
        }

        var usage = new UsageRecord
        {
            UserId = userId,
            ModelId = model.Id,
            Model = model,
            OptimizationId = optimization.Id,
            Optimization = optimization,
            InputTokens = response.TokenUsage.PromptTokens,
            OutputTokens = response.TokenUsage.CompletionTokens,
            TotalTokens = response.TokenUsage.TotalTokens,
            EstimatedCost = estimatedCost
        };

        _context.Optimizations.Add(optimization);
        _context.UsageRecords.Add(usage);
        await _context.SaveChangesAsync(cancellationToken);

        return new OptimizationResultDto(
            optimization.Id, prompt.Id, prompt.OriginalContent, optimization.OptimizedContent,
            model.Id, model.Name, model.ModelIdentifier, provider.Code, optimization.ProcessingTimeMs,
            optimization.OriginalTokens, optimization.OptimizedTokens, optimization.TokensSaved,
            optimization.ReductionPercentage, usage.InputTokens, usage.OutputTokens, usage.TotalTokens,
            usage.EstimatedCost, optimization.CreatedAt);
    }
}
