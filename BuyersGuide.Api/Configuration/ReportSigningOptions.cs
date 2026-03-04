namespace BuyersGuide.Api.Configuration;

/// <summary>
/// Configuration options for HMAC-SHA256 report URL signing.
/// Bound from appsettings.json section "ReportSigning".
/// </summary>
public class ReportSigningOptions
{
    public const string SectionName = "ReportSigning";

    /// <summary>
    /// HMAC secret key. MUST be configured in production.
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// Number of minutes before a signed URL expires. Default 15.
    /// </summary>
    public int ExpiryMinutes { get; set; } = 15;
}
