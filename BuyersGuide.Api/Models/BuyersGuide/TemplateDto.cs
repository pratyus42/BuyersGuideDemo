namespace BuyersGuide.Api.Models.BuyersGuide;

/// <summary>
/// Represents a single Buyers Guide template available for a dealer.
/// </summary>
public class TemplateDto
{
    /// <summary>
    /// Unique template identifier (e.g., "TMP001").
    /// </summary>
    public string TemplateId { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable template name.
    /// </summary>
    public string TemplateName { get; set; } = string.Empty;

    /// <summary>
    /// Identifier for the printable version of the template.
    /// </summary>
    public string PrintableTemplateId { get; set; } = string.Empty;
}
