using BuyersGuide.Api.Helpers;
using BuyersGuide.Api.Services.Interfaces;

namespace BuyersGuide.Api.Services.Implementations;

/// <summary>
/// Mock implementation of IReportApiClient.
/// Generates a signed report URL locally using HMAC-SHA256.
/// In production, this would call an external report generation service.
/// IMPORTANT: The generated URL MUST NOT be logged (it is a secret).
/// </summary>
public class ReportApiClient : IReportApiClient
{
    private readonly ReportUrlSigner _urlSigner;
    private readonly ILogger<ReportApiClient> _logger;

    public ReportApiClient(ReportUrlSigner urlSigner, ILogger<ReportApiClient> logger)
    {
        _urlSigner = urlSigner;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<string> GenerateReportAsync(string templateId, string vin)
    {
        // Generate a unique report ID
        var reportId = $"RPT-{Guid.NewGuid():N}";

        _logger.LogInformation("Generating report {ReportId} for template {TemplateId}",
            reportId, templateId);
        // DO NOT log the VIN or report URL here

        var signedUrl = _urlSigner.GenerateSignedUrl(reportId, templateId, vin);

        // DO NOT log signedUrl — it is a secret
        return Task.FromResult(signedUrl);
    }
}
