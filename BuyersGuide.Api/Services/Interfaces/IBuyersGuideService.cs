using BuyersGuide.Api.Models.BuyersGuide;

namespace BuyersGuide.Api.Services.Interfaces;

/// <summary>
/// Core service contract for BuyersGuide operations.
/// </summary>
public interface IBuyersGuideService
{
    /// <summary>
    /// Returns the list of templates available for the specified dealer.
    /// </summary>
    /// <param name="dealerId">The dealer identifier.</param>
    /// <returns>Template response containing dealer-scoped templates.</returns>
    Task<TemplateResponse> GetTemplatesAsync(string dealerId);

    /// <summary>
    /// Generates a BuyersGuide report for the given template and VIN.
    /// Validates that the template belongs to the specified dealer.
    /// </summary>
    /// <param name="dealerId">The authenticated dealer identifier.</param>
    /// <param name="request">Report generation request.</param>
    /// <returns>Report response with signed URL, or null if template not found for dealer.</returns>
    Task<ReportResponse?> GenerateReportAsync(string dealerId, ReportRequest request);
}
