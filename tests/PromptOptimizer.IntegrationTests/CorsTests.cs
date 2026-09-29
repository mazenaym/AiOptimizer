using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using PromptOptimizer.Api.Contracts;

namespace PromptOptimizer.IntegrationTests;

public class CorsTests
{
    private const string AllowedOrigin = "https://frontend.example.test";

    [Theory]
    [InlineData("GET")]
    [InlineData("POST")]
    [InlineData("DELETE")]
    public async Task Configured_origin_can_preflight_authenticated_api_requests(string method)
    {
        await using var factory = new OptimizationApiFactory { AllowedOrigins = [AllowedOrigin] };
        using var client = CreateClient(factory);
        using var request = Preflight(AllowedOrigin, method);
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(AllowedOrigin, response.Headers.GetValues("Access-Control-Allow-Origin").Single());
        Assert.Contains(method, string.Join(",", response.Headers.GetValues("Access-Control-Allow-Methods")));
        var headers = string.Join(",", response.Headers.GetValues("Access-Control-Allow-Headers")).ToLowerInvariant();
        Assert.Contains("authorization", headers);
        Assert.Contains("content-type", headers);
        Assert.False(response.Headers.Contains("Access-Control-Allow-Credentials"));
        Assert.Equal(0, factory.Engine.Calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Unconfigured_origin_is_denied_including_empty_production_allowlist(bool emptyList)
    {
        await using var factory = new OptimizationApiFactory { AllowedOrigins = emptyList ? [] : [AllowedOrigin] };
        using var client = CreateClient(factory);
        using var request = Preflight("https://untrusted.example.test", "POST");
        using var response = await client.SendAsync(request);
        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
        Assert.False(response.Headers.Contains("Access-Control-Allow-Credentials"));
        using var actual = new HttpRequestMessage(HttpMethod.Get, "/api/health");
        actual.Headers.Add("Origin", "https://untrusted.example.test");
        using var actualResponse = await client.SendAsync(actual);
        Assert.False(actualResponse.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task Allowed_origin_receives_cors_headers_on_authentication_errors_without_bypassing_auth()
    {
        await using var factory = new OptimizationApiFactory { AllowedOrigins = [AllowedOrigin] };
        using var client = CreateClient(factory);
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/prompts/{Guid.NewGuid()}/optimize")
        {
            Content = JsonContent.Create(new OptimizePromptRequest(Guid.NewGuid()))
        };
        request.Headers.Add("Origin", AllowedOrigin);
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(AllowedOrigin, response.Headers.GetValues("Access-Control-Allow-Origin").Single());
        Assert.Equal(0, factory.Engine.Calls);
    }

    private static HttpClient CreateClient(OptimizationApiFactory factory) => factory.CreateClient(
        new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });

    private static HttpRequestMessage Preflight(string origin, string method)
    {
        var request = new HttpRequestMessage(HttpMethod.Options, $"/api/prompts/{Guid.NewGuid()}/optimize");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", method);
        request.Headers.Add("Access-Control-Request-Headers", "authorization,content-type");
        return request;
    }
}
