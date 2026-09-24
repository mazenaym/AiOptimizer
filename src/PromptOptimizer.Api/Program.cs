using Microsoft.EntityFrameworkCore;
using PromptOptimizer.Api.Extensions;
using PromptOptimizer.Api.Middleware;
using PromptOptimizer.Application;
using PromptOptimizer.Application.Common.Interfaces;
using PromptOptimizer.Infrastructure;
using PromptOptimizer.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Register layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);
//db
// تسجيل الـ DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ربط الـ Interface بـ AppDbContext
builder.Services.AddScoped<IAppDbContext>(provider => (IAppDbContext)provider.GetRequiredService<AppDbContext>());

var app = builder.Build();


// Ensure DB is initialized & seeded
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    
    dbContext.Database.Migrate();
}
// Custom Middlewares
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<RateLimitMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PromptOptimizer API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Make Program class public for Integration Tests
public partial class Program { }
