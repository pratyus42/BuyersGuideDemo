namespace BuyersGuide.Api.Models.BuyersGuide;

/// <summary>
/// Response payload for POST /api/BuyersGuide/report.
/// </summary>
public class ReportResponse
{
    /// <summary>
    /// The VIN used for report generation.
    /// </summary>
    public string Vin { get; set; } = string.Empty;

    /// <summary>
    /// The template used for report generation.
    /// </summary>
    public string TemplateId { get; set; } = string.Empty;

    /// <summary>
    /// Signed, time-bounded URL for downloading the report.
    /// IMPORTANT: This value is a secret and MUST NOT be logged.
    /// </summary>
    public string ReportUrl { get; set; } = string.Empty;
}
