//using FluentValidation;
//using MediatR;
//using Microsoft.EntityFrameworkCore;
//using PromptOptimizer.Application.Common.Exceptions;
//using PromptOptimizer.Application.Common.Interfaces;
//using PromptOptimizer.Application.Common.Results;
//using PromptOptimizer.Application.Optimization.DTOs;
//using PromptOptimizer.Application.Optimization.Models;
//using PromptOptimizer.Application.Optimization.Services;
//using PromptOptimizer.Domain.Entities;
//using PromptOptimizer.Domain.Enums;
//using DomainOptimization = PromptOptimizer.Domain.Entities.Optimization;

//namespace PromptOptimizer.Application.Prompts.Commands.OptimizePrompt;

//public record OptimizePromptCommand(
//    Guid? PromptId,
//    string Content,
//    PromptCategory Category,
//    string Provider,
//    string ModelKey,
//    string? CustomInstructions
//) : IRequest<Result<OptimizationResultDto>>;

//public class OptimizePromptCommandValidator : AbstractValidator<OptimizePromptCommand>
//{
//    public OptimizePromptCommandValidator()
//    {
//        RuleFor(x => x.Content).NotEmpty().MaximumLength(10000);
//        RuleFor(x => x.Provider).NotEmpty();
//        RuleFor(x => x.ModelKey).NotEmpty();
//    }
//}

//public class OptimizePromptCommandHandler : IRequestHandler<OptimizePromptCommand, Result<OptimizationResultDto>>
//{
//    private readonly IAppDbContext _context;
//    private readonly IPromptOptimizerEngine _optimizerEngine;
//    private readonly ICurrentUserService _currentUserService;

//    public OptimizePromptCommandHandler(
//        IAppDbContext context,
//        IPromptOptimizerEngine optimizerEngine,
//        ICurrentUserService currentUserService)
//    {
//        _context = context;
//        _optimizerEngine = optimizerEngine;
//        _currentUserService = currentUserService;
//    }

//    public async Task<Result<OptimizationResultDto>> Handle(OptimizePromptCommand request, CancellationToken cancellationToken)
//    {
//        var userId = _currentUserService.UserId;
//        if (!userId.HasValue)
//        {
//            return Result.Failure<OptimizationResultDto>("User is not authenticated.");
//        }

//        Prompt prompt;
//        if (request.PromptId.HasValue && request.PromptId.Value != Guid.Empty)
//        {
//            prompt = await _context.Prompts.FirstOrDefaultAsync(p => p.Id == request.PromptId.Value && p.UserId == userId.Value, cancellationToken)
//                ?? throw new NotFoundException(nameof(Prompt), request.PromptId.Value);
//        }
//        else
//        {
//            prompt = new Prompt
//            {
//                UserId = userId.Value,
//                Title = request.Content.Length > 30 ? request.Content[..30] + "..." : request.Content,
//                Content = request.Content,
//                Category = request.Category,
//                CreatedBy = userId.Value.ToString()
//            };
//            _context.Prompts.Add(prompt);
//        }

//        var aiModel = await _context.AIModels.FirstOrDefaultAsync(m => m.ModelKey == request.ModelKey, cancellationToken);
//        if (aiModel == null)
//        {
//            aiModel = new AIModel
//            {
//                Name = request.ModelKey,
//                ModelKey = request.ModelKey,
//                Provider = request.Provider,
//                ContextWindow = 128000,
//                InputCostPer1k = 0.0015m,
//                OutputCostPer1k = 0.002m
//            };
//            _context.AIModels.Add(aiModel);
//        }

//        var optimizationReq = new PromptOptimizationRequest(
//            request.Content,
//            request.Category,
//            request.Provider,
//            request.ModelKey,
//            request.CustomInstructions
//        );

//        var response = await _optimizerEngine.OptimizeAsync(optimizationReq, cancellationToken);

//        var optimization = new DomainOptimization
//        {
//            Prompt = prompt,
//            AIModel = aiModel,
//            SystemInstructions = request.CustomInstructions ?? string.Empty,
//            OriginalContent = request.Content,
//            OptimizedContent = response.OptimizedContent,
//            Explanation = response.Explanation,
//            Status = OptimizationStatus.Completed,
//            TokenUsage = response.TokenUsage,
//            ExecutionTimeMs = response.ExecutionTimeMs,
//            CreatedBy = userId.Value.ToString()
//        };

//        _context.Optimizations.Add(optimization);

//        var usageRecord = new UsageRecord
//        {
//            UserId = userId.Value,
//            Optimization = optimization,
//            TokenUsage = response.TokenUsage,
//            EstimatedCost = (response.TokenUsage.PromptTokens * aiModel.InputCostPer1k / 1000m) +
//                            (response.TokenUsage.CompletionTokens * aiModel.OutputCostPer1k / 1000m)
//        };

//        _context.UsageRecords.Add(usageRecord);

//        await _context.SaveChangesAsync(cancellationToken);

//        var resultDto = new OptimizationResultDto(
//            optimization.Id,
//            prompt.Id,
//            optimization.OriginalContent,
//            optimization.OptimizedContent,
//            optimization.Explanation,
//            optimization.Status,
//            optimization.TokenUsage.PromptTokens,
//            optimization.TokenUsage.CompletionTokens,
//            optimization.TokenUsage.TotalTokens,
//            optimization.ExecutionTimeMs,
//            aiModel.Name,
//            optimization.CreatedAt
//        );

//        return Result.Success(resultDto);
//    }
//}
