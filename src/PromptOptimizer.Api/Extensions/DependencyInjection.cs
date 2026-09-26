using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PromptOptimizer.Api.Contracts;
using PromptOptimizer.Api.Services;
using PromptOptimizer.Application.Common.Interfaces;

namespace PromptOptimizer.Api.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Fail early when JWT configuration is missing or invalid.
        var key = GetRequiredSetting(configuration, "Jwt:Key");
        var issuer = GetRequiredSetting(configuration, "Jwt:Issuer");
        var audience = GetRequiredSetting(configuration, "Jwt:Audience");

        if (Encoding.UTF8.GetByteCount(key) < 32)
        {
            throw new InvalidOperationException(
                "Jwt:Key must contain at least 32 UTF-8 bytes for HS256.");
        }

        if (!int.TryParse(
                configuration["Jwt:AccessTokenExpirationMinutes"],
                out var expirationMinutes)
            || expirationMinutes <= 0)
        {
            throw new InvalidOperationException(
                "Jwt:AccessTokenExpirationMinutes must be a positive integer.");
        }

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(entry =>
                            entry.Value is { Errors.Count: > 0 })
                        .ToDictionary(
                            entry => entry.Key,
                            entry => entry.Value!.Errors
                                .Select(error =>
                                    string.IsNullOrWhiteSpace(error.ErrorMessage)
                                        ? "Invalid value."
                                        : error.ErrorMessage)
                                .ToArray());

                    var response = new ApiErrorResponse(
                        StatusCodes.Status400BadRequest,
                        "One or more validation errors occurred.",
                        errors,
                        context.HttpContext.TraceIdentifier);

                    return new BadRequestObjectResult(response);
                };
            });

        services.AddSwaggerConfiguration();

        services.AddAuthentication(
                JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = issuer,
                        ValidAudience = audience,

                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(key)),

                        ValidAlgorithms =
                            new[] { SecurityAlgorithms.HmacSha256 },

                        ClockSkew = TimeSpan.FromSeconds(30)
                    };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        // Replace the default authentication response.
                        context.HandleResponse();

                        if (context.Response.HasStarted)
                            return;

                        context.Response.StatusCode =
                            StatusCodes.Status401Unauthorized;

                        context.Response.Headers["WWW-Authenticate"] =
                            "Bearer";

                        await context.Response.WriteAsJsonAsync(
                            new ApiErrorResponse(
                                StatusCodes.Status401Unauthorized,
                                "Authentication is required or the token is invalid.",
                                null,
                                context.HttpContext.TraceIdentifier),
                            cancellationToken:
                                context.HttpContext.RequestAborted);
                    },

                    OnForbidden = async context =>
                    {
                        if (context.Response.HasStarted)
                            return;

                        context.Response.StatusCode =
                            StatusCodes.Status403Forbidden;

                        await context.Response.WriteAsJsonAsync(
                            new ApiErrorResponse(
                                StatusCodes.Status403Forbidden,
                                "You do not have permission to access this resource.",
                                null,
                                context.HttpContext.TraceIdentifier),
                            cancellationToken:
                                context.HttpContext.RequestAborted);
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }

    private static string GetRequiredSetting(
        IConfiguration configuration,
        string settingName)
    {
        var value = configuration[settingName];

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Required configuration '{settingName}' is missing or empty.");
        }

        return value;
    }
}