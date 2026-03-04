using BuyersGuide.Api.Models.BuyersGuide;
using BuyersGuide.Api.Services.Interfaces;

namespace BuyersGuide.Api.Services.Implementations;

/// <summary>
/// In-memory implementation of IBuyersGuideService.
/// Uses a static dealer→templates mapping for mock/dev scenarios.
/// </summary>
public class BuyersGuideService : IBuyersGuideService
{
    private readonly IReportApiClient _reportApiClient;
    private readonly ILogger<BuyersGuideService> _logger;

    /// <summary>
    /// In-memory dealer→templates mapping per spec:
    /// D1001 → TMP001, TMP002
    /// D2001 → TMP003
    /// </summary>
    private static readonly Dictionary<string, List<TemplateDto>> DealerTemplates = new()
    {
        ["D1001"] = new List<TemplateDto>
        {
            new() { TemplateId = "TMP001", TemplateName = "Standard Buyers Guide", PrintableTemplateId = "PRT001" },
            new() { TemplateId = "TMP002", TemplateName = "Warranty Buyers Guide", PrintableTemplateId = "PRT002" }
        },
        ["D2001"] = new List<TemplateDto>
        {
            new() { TemplateId = "TMP003", TemplateName = "Premium Buyers Guide", PrintableTemplateId = "PRT003" }
        }
    };

    public BuyersGuideService(IReportApiClient reportApiClient, ILogger<BuyersGuideService> logger)
    {
        _reportApiClient = reportApiClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<TemplateResponse> GetTemplatesAsync(string dealerId)
    {
        _logger.LogInformation("Fetching templates for dealer {DealerId}", dealerId);

        var templates = DealerTemplates.TryGetValue(dealerId, out var list)
            ? list
            : new List<TemplateDto>();

        var response = new TemplateResponse
        {
            DealerId = dealerId,
            Templates = templates
        };

        return Task.FromResult(response);
    }

    /// <inheritdoc />
    public async Task<ReportResponse?> GenerateReportAsync(string dealerId, ReportRequest request)
    {
        _logger.LogInformation("Generating report for dealer {DealerId}, template {TemplateId}",
            dealerId, request.TemplateId);

        // Verify the template belongs to this dealer
        if (!DealerTemplates.TryGetValue(dealerId, out var templates) ||
            !templates.Any(t => t.TemplateId == request.TemplateId))
        {
            _logger.LogWarning("Template {TemplateId} not found for dealer {DealerId}",
                request.TemplateId, dealerId);
            return null;
        }

        // Generate signed report URL (DO NOT log the URL - it's a secret)
        var reportUrl = await _reportApiClient.GenerateReportAsync(request.TemplateId, request.Vin);

        return new ReportResponse
        {
            Vin = request.Vin,
            TemplateId = request.TemplateId,
            ReportUrl = reportUrl
        };
    }
}
