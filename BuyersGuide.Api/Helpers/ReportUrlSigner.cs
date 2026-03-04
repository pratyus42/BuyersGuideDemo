using System.Security.Cryptography;
using System.Text;
using BuyersGuide.Api.Configuration;
using Microsoft.Extensions.Options;

namespace BuyersGuide.Api.Helpers;

/// <summary>
/// Signs report URLs using HMAC-SHA256.
/// Format: {base}?reportId={reportId}&amp;templateId={templateId}&amp;vin={vin}&amp;exp={unixTimestamp}&amp;signature={hmac}
/// </summary>
public class ReportUrlSigner
{
    private readonly ReportSigningOptions _options;

    public ReportUrlSigner(IOptions<ReportSigningOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// Generates a signed, time-bounded URL for a report.
    /// The URL points to the local download endpoint.
    /// </summary>
    public string GenerateSignedUrl(string reportId, string templateId, string vin)
    {
        return GenerateSignedUrl(reportId, templateId, vin, _options.ExpiryMinutes);
    }

    /// <summary>
    /// Generates a signed, time-bounded URL with a custom expiry.
    /// </summary>
    public string GenerateSignedUrl(string reportId, string templateId, string vin, int expiryMinutes)
    {
        var expiry = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes).ToUnixTimeSeconds();
        var dataToSign = $"{reportId}.{templateId}.{vin}.{expiry}";

        var signature = ComputeHmacSha256(dataToSign, _options.Secret);

        // Use a relative path so the URL resolves to the local API download endpoint
        return $"/api/BuyersGuide/report/download?reportId={reportId}&templateId={templateId}&vin={vin}&exp={expiry}&signature={signature}";
    }

    /// <summary>
    /// Validates the HMAC signature and checks expiry.
    /// </summary>
    /// <returns>True if signature is valid and URL has not expired.</returns>
    public bool ValidateSignature(string reportId, string templateId, string vin, long exp, string signature)
    {
        // Check expiry
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (exp < now)
            return false;

        // Recompute signature
        var dataToSign = $"{reportId}.{templateId}.{vin}.{exp}";
        var expected = ComputeHmacSha256(dataToSign, _options.Secret);

        return string.Equals(signature, expected, StringComparison.OrdinalIgnoreCase);
    }

    private static string ComputeHmacSha256(string data, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
