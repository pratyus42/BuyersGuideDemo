using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc;

namespace BuyersGuide.Api.Tests;

/// <summary>
/// Integration tests for POST /api/BuyersGuide/report endpoint (happy path).
/// </summary>
public class ReportEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ReportEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GenerateReport_WithValidRequest_ReturnsReportUrl()
    {
        // Arrange
        var requestBody = new { templateId = "TMP001", vin = "1HGCM82633A123456" };
        var content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/BuyersGuide/report")
        {
            Content = content
        };
        request.Headers.Add("X-Api-Key", "test-key");
        request.Headers.Add("X-Dealer-Id", "D1001");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ReportResponseDto>();
        Assert.NotNull(body);
        Assert.Equal("1HGCM82633A123456", body!.Vin);
        Assert.Equal("TMP001", body.TemplateId);
        Assert.NotNull(body.ReportUrl);
        Assert.NotEmpty(body.ReportUrl);

        // Verify URL contains expected components
        Assert.Contains("signature=", body.ReportUrl);
        Assert.Contains("exp=", body.ReportUrl);
        Assert.Contains("/api/BuyersGuide/report/download", body.ReportUrl);
    }

    [Fact]
    public async Task GenerateReport_TemplateNotForDealer_Returns403()
    {
        // Arrange: TMP003 belongs to D2001, not D1001
        var requestBody = new { templateId = "TMP003", vin = "1HGCM82633A123456" };
        var content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/BuyersGuide/report")
        {
            Content = content
        };
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
    public async Task GenerateReport_WithoutAuth_Returns401()
    {
        // Arrange
        var requestBody = new { templateId = "TMP001", vin = "1HGCM82633A123456" };
        var content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/BuyersGuide/report")
        {
            Content = content
        };
        // Deliberately no auth headers

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GenerateReport_ResponseIncludesCorrelationId()
    {
        // Arrange
        var requestBody = new { templateId = "TMP001", vin = "1HGCM82633A123456" };
        var content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/BuyersGuide/report")
        {
            Content = content
        };
        request.Headers.Add("X-Api-Key", "test-key");
        request.Headers.Add("X-Dealer-Id", "D1001");
        request.Headers.Add("X-Correlation-Id", "report-corr-456");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Correlation-Id"));
        Assert.Equal("report-corr-456", response.Headers.GetValues("X-Correlation-Id").First());
    }

    // Helper DTO for deserialization
    private record ReportResponseDto
    {
        public string Vin { get; init; } = string.Empty;
        public string TemplateId { get; init; } = string.Empty;
        public string ReportUrl { get; init; } = string.Empty;
    }
}
