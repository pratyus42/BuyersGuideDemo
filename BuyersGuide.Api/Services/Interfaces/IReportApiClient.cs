namespace BuyersGuide.Api.Services.Interfaces;

/// <summary>
/// Client contract for generating report URLs via an external (mock) report service.
/// </summary>
public interface IReportApiClient
{
    /// <summary>
    /// Generates a signed report URL for a given template and VIN.
    /// </summary>
    /// <param name="templateId">The template to use for report generation.</param>
    /// <param name="vin">The vehicle identification number.</param>
    /// <returns>A signed, time-bounded report URL.</returns>
    Task<string> GenerateReportAsync(string templateId, string vin);
}
