using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using PromptOptimizer.Application.Common.Interfaces;

namespace PromptOptimizer.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<TRequest> _logger;
    private readonly ICurrentUserService _currentUserService;

    public LoggingBehavior(
        ILogger<TRequest> logger,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        var userId = _currentUserService.UserId?.ToString()
            ?? "Anonymous";

        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Handling {RequestName} for user {UserId}",
            requestName,
            userId);

        var response = await next();

        stopwatch.Stop();

        _logger.LogInformation(
            "Handled {RequestName} for user {UserId} in {ElapsedMs} ms",
            requestName,
            userId,
            stopwatch.ElapsedMilliseconds);

        return response;
    }
}