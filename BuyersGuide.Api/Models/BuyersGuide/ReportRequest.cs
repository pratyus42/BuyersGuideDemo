using System.ComponentModel.DataAnnotations;

namespace BuyersGuide.Api.Models.BuyersGuide;

/// <summary>
/// Request payload for POST /api/BuyersGuide/report.
/// </summary>
public class ReportRequest
{
    /// <summary>
    /// The template identifier to use for report generation.
    /// </summary>
    [Required(ErrorMessage = "templateId is required.")]
    public string TemplateId { get; set; } = string.Empty;

    /// <summary>
    /// The Vehicle Identification Number (17 characters; excludes I, O, Q).
    /// </summary>
    [Required(ErrorMessage = "vin is required.")]
    [RegularExpression(@"^[A-HJ-NPR-Z0-9]{17}$",
        ErrorMessage = "vin must be exactly 17 characters and must not contain I, O, or Q.")]
    public string Vin { get; set; } = string.Empty;
}
