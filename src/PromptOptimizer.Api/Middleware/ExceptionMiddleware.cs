using System.Net;
using System.Text.Json;
using PromptOptimizer.Application.Common.Exceptions;

namespace PromptOptimizer.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var statusCode = HttpStatusCode.InternalServerError;
        var response = new Dictionary<string, object>
        {
            { "message", exception.Message }
        };

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = HttpStatusCode.BadRequest;
                response["errors"] = validationException.Errors;
                break;
            case NotFoundException:
                statusCode = HttpStatusCode.NotFound;
                break;
            case UnauthorizedException:
                statusCode = HttpStatusCode.Unauthorized;
                break;
        }

        context.Response.StatusCode = (int)statusCode;
        var result = JsonSerializer.Serialize(response);
        return context.Response.WriteAsync(result);
    }
}
