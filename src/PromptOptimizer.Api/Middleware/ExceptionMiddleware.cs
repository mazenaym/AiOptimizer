using PromptOptimizer.Api.Contracts;
using PromptOptimizer.Application.Common.Exceptions;

namespace PromptOptimizer.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // The client disconnected; do not turn cancellation into an error response.
            throw;
        }
        catch (Exception exception)
        {
            // لا يمكن تغيير الاستجابة بعد بدء إرسالها.
            if (context.Response.HasStarted)
            {
                throw;
            }

            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var statusCode = StatusCodes.Status500InternalServerError;

        var message =
            "An unexpected error occurred. Please try again later.";

        IReadOnlyDictionary<string, string[]>? errors = null;

        switch (exception)
        {
            case AIProviderException provider:
                // Use fixed public messages, never an upstream or exception message.
                (statusCode, message) = provider.Failure switch
                {
                    AIProviderFailure.RateLimit => (
                        StatusCodes.Status429TooManyRequests,
                        "The AI service is rate limited. Please try again later."),
                    AIProviderFailure.InvalidResponse => (
                        StatusCodes.Status502BadGateway,
                        "The AI service returned an invalid response. Please try again later."),
                    _ => (
                        StatusCodes.Status503ServiceUnavailable,
                        "The AI service is currently unavailable. Please try again later.")
                };
                break;

            case ConflictException conflict:
                statusCode = StatusCodes.Status409Conflict;
                message = conflict.Message;
                break;

            case ValidationException validation:
                statusCode = StatusCodes.Status400BadRequest;
                message = validation.Message;
                errors = validation.Errors;
                break;

            case UnauthorizedException unauthorized:
                statusCode = StatusCodes.Status401Unauthorized;
                message = unauthorized.Message;
                break;

            case NotFoundException notFound:
                statusCode = StatusCodes.Status404NotFound;
                message = notFound.Message;
                break;
        }

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Unexpected error. TraceId: {TraceId}",
                context.TraceIdentifier);
        }
        else
        {
            _logger.LogWarning(
                "Request rejected with status {StatusCode}. " +
                "ExceptionType: {ExceptionType}. TraceId: {TraceId}",
                statusCode,
                exception.GetType().Name,
                context.TraceIdentifier);
        }

        var response = new ApiErrorResponse(
    statusCode,
    message,
    errors,
    context.TraceIdentifier);

        context.Response.Clear();
        context.Response.StatusCode = statusCode;

        if (statusCode == StatusCodes.Status401Unauthorized)
        {
            context.Response.Headers["WWW-Authenticate"] = "Bearer";
        }

        await context.Response.WriteAsJsonAsync(
            response,
            cancellationToken: context.RequestAborted);
    }
}
