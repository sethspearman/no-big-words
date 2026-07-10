using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using NoBigWords.Core.Models;

namespace NoBigWords.Tests;

public sealed class RewriteApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public RewriteApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RewriteEndpoint_RewritesKnownStrictModeTerms()
    {
        var response = await _client.PostAsJsonAsync("/api/rewrite", new RewriteRequest(
            "The astronaut used a telescope to observe the planet.",
            RewriteMode.NoBigWords));

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<RewriteResponse>();

        Assert.NotNull(payload);
        Assert.Contains("space person", payload!.RewrittenText);
        Assert.Contains("far-seeing thing", payload.RewrittenText);
        Assert.Contains("world", payload.RewrittenText);
        Assert.NotEmpty(payload.Replacements);
    }

    [Fact]
    public async Task RewriteEndpoint_ReturnsBadRequestForBlankText()
    {
        var response = await _client.PostAsJsonAsync("/api/rewrite", new RewriteRequest(
            "",
            RewriteMode.NoBigWords));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ValidateEndpoint_ReturnsValidationSummary()
    {
        var response = await _client.PostAsJsonAsync("/api/validate", new RewriteRequest(
            "planet",
            RewriteMode.NoBigWords));

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ValidationSummary>();

        Assert.NotNull(payload);
        Assert.True(payload!.TotalWords >= 1);
        Assert.True(payload.DisallowedWords >= 0);
    }
}
