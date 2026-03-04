using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc;

namespace BuyersGuide.Api.Tests;

/// <summary>
/// Integration tests for POST /api/BuyersGuide/report validation behavior.
/// </summary>
public class ReportEndpointValidationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ReportEndpointValidationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GenerateReport_MissingVin_Returns422()
    {
        // Arrange
        var requestBody = new { templateId = "TMP001" };
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
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task GenerateReport_MissingTemplateId_Returns422()
    {
        // Arrange
        var requestBody = new { vin = "1HGCM82633A123456" };
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
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Theory]
    [InlineData("12345")]              // too short
    [InlineData("1HGCM82633A12345I")]  // contains I
    [InlineData("1HGCM82633A12345O")]  // contains O
    [InlineData("1HGCM82633A12345Q")]  // contains Q
    [InlineData("")]                   // empty
    public async Task GenerateReport_InvalidVin_Returns422(string invalidVin)
    {
        // Arrange
        var requestBody = new { templateId = "TMP001", vin = invalidVin };
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
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("vin", responseContent, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GenerateReport_EmptyBody_Returns422OrBadRequest()
    {
        // Arrange
        var content = new StringContent("{}", Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/BuyersGuide/report")
        {
            Content = content
        };
        request.Headers.Add("X-Api-Key", "test-key");
        request.Headers.Add("X-Dealer-Id", "D1001");

        // Act
        var response = await _client.SendAsync(request);

        // Assert — empty body hits required validation
        var statusCode = (int)response.StatusCode;
        Assert.True(statusCode == 422 || statusCode == 400,
            $"Expected 422 or 400 but got {statusCode}");
    }
}
