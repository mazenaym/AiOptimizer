using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PromptOptimizer.Api.Services;
using PromptOptimizer.Application.Common.Interfaces;

namespace PromptOptimizer.Api.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddControllers();
        services.AddSwaggerConfiguration();

        var secret = configuration["Jwt:Secret"] ?? "SuperSecretKeyForPromptOptimizerNet10Core2026!";
        var issuer = configuration["Jwt:Issuer"] ?? "PromptOptimizerApi";
        var audience = configuration["Jwt:Audience"] ?? "PromptOptimizerUsers";

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
                };
            });

        services.AddAuthorization();

        return services;
    }
}
