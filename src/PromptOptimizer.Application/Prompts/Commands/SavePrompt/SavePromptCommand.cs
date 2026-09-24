//using FluentValidation;
//using MediatR;
//using Microsoft.EntityFrameworkCore;
//using PromptOptimizer.Application.Common.Exceptions;
//using PromptOptimizer.Application.Common.Interfaces;
//using PromptOptimizer.Application.Common.Results;
//using PromptOptimizer.Application.Prompts.DTOs;
//using PromptOptimizer.Domain.Entities;
//using PromptOptimizer.Domain.Enums;

//namespace PromptOptimizer.Application.Prompts.Commands.SavePrompt;

//public record SavePromptCommand(
//    Guid? Id,
//    string Title,
//    string Content,
//    PromptCategory Category,
//    bool IsFavorite,
//    List<string>? Tags
//) : IRequest<Result<PromptDto>>;

//public class SavePromptCommandValidator : AbstractValidator<SavePromptCommand>
//{
//    public SavePromptCommandValidator()
//    {
//        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
//        RuleFor(x => x.Content).NotEmpty();
//    }
//}

//public class SavePromptCommandHandler : IRequestHandler<SavePromptCommand, Result<PromptDto>>
//{
//    private readonly IAppDbContext _context;
//    private readonly ICurrentUserService _currentUserService;

//    public SavePromptCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
//    {
//        _context = context;
//        _currentUserService = currentUserService;
//    }

//    public async Task<Result<PromptDto>> Handle(SavePromptCommand request, CancellationToken cancellationToken)
//    {
//        var userId = _currentUserService.UserId;
//        if (!userId.HasValue)
//        {
//            return Result.Failure<PromptDto>("Unauthorized");
//        }

//        Prompt prompt;
//        if (request.Id.HasValue && request.Id.Value != Guid.Empty)
//        {
//            prompt = await _context.Prompts.FirstOrDefaultAsync(p => p.Id == request.Id.Value && p.UserId == userId.Value, cancellationToken)
//                ?? throw new NotFoundException(nameof(Prompt), request.Id.Value);

//            prompt.Title = request.Title;
//            prompt.Content = request.Content;
//            prompt.Category = request.Category;
//            prompt.IsFavorite = request.IsFavorite;
//            prompt.Tags = request.Tags ?? new();
//            prompt.LastModifiedAt = DateTime.UtcNow;
//            prompt.LastModifiedBy = userId.Value.ToString();
//        }
//        else
//        {
//            prompt = new Prompt
//            {
//                UserId = userId.Value,
//                Title = request.Title,
//                Content = request.Content,
//                Category = request.Category,
//                IsFavorite = request.IsFavorite,
//                Tags = request.Tags ?? new(),
//                CreatedBy = userId.Value.ToString()
//            };
//            _context.Prompts.Add(prompt);
//        }

//        await _context.SaveChangesAsync(cancellationToken);

//        var dto = new PromptDto(
//            prompt.Id,
//            prompt.UserId,
//            prompt.Title,
//            prompt.Content,
//            prompt.Category,
//            prompt.IsFavorite,
//            prompt.Tags,
//            prompt.CreatedAt,
//            prompt.LastModifiedAt
//        );

//        return Result.Success(dto);
//    }
//}
