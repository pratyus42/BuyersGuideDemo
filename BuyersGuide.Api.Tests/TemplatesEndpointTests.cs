using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc;

namespace BuyersGuide.Api.Tests;

/// <summary>
/// Integration tests for GET /api/BuyersGuide/templates endpoint.
/// </summary>
public class TemplatesEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TemplatesEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTemplates_WithValidDealerHeaders_ReturnsTemplates()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/BuyersGuide/templates?dealerId=D1001");
        request.Headers.Add("X-Api-Key", "test-key");
        request.Headers.Add("X-Dealer-Id", "D1001");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<TemplateResponseDto>();
        Assert.NotNull(body);
        Assert.Equal("D1001", body!.DealerId);
        Assert.NotNull(body.Templates);
        Assert.Equal(2, body.Templates.Count);

        Assert.Contains(body.Templates, t => t.TemplateId == "TMP001");
        Assert.Contains(body.Templates, t => t.TemplateId == "TMP002");
    }

    [Fact]
    public async Task GetTemplates_WithMismatchedDealerId_Returns403()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/BuyersGuide/templates?dealerId=D9999");
        request.Headers.Add("X-Api-Key", "test-key");
        request.Headers.Add("X-Dealer-Id", "D1001");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(403, problem!.Status);
    }

    [Fact]
    public async Task GetTemplates_WithoutApiKey_Returns401()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/BuyersGuide/templates?dealerId=D1001");
        // Deliberately no X-Api-Key header

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetTemplates_MissingDealerId_Returns400()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/BuyersGuide/templates");
        request.Headers.Add("X-Api-Key", "test-key");
        request.Headers.Add("X-Dealer-Id", "D1001");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetTemplates_UnknownDealer_ReturnsEmptyTemplateList()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/BuyersGuide/templates?dealerId=DUNKNOWN");
        request.Headers.Add("X-Api-Key", "test-key");
        request.Headers.Add("X-Dealer-Id", "DUNKNOWN");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<TemplateResponseDto>();
        Assert.NotNull(body);
        Assert.Equal("DUNKNOWN", body!.DealerId);
        Assert.Empty(body.Templates);
    }

    [Fact]
    public async Task GetTemplates_ResponseIncludesCorrelationId()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/BuyersGuide/templates?dealerId=D1001");
        request.Headers.Add("X-Api-Key", "test-key");
        request.Headers.Add("X-Dealer-Id", "D1001");
        request.Headers.Add("X-Correlation-Id", "test-corr-123");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Correlation-Id"));
        Assert.Equal("test-corr-123", response.Headers.GetValues("X-Correlation-Id").First());
    }

    // Helper DTOs for deserialization
    private record TemplateResponseDto
    {
        public string DealerId { get; init; } = string.Empty;
        public List<TemplateDtoRecord> Templates { get; init; } = new();
    }

    private record TemplateDtoRecord
    {
        public string TemplateId { get; init; } = string.Empty;
        public string TemplateName { get; init; } = string.Empty;
        public string PrintableTemplateId { get; init; } = string.Empty;
    }
}
