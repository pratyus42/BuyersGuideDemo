using BuyersGuide.Api.Configuration;
using BuyersGuide.Api.Helpers;
using Microsoft.Extensions.Options;

namespace BuyersGuide.Api.Tests;

/// <summary>
/// Unit tests for ReportUrlSigner (HMAC-SHA256 signature + expiry).
/// </summary>
public class ReportUrlSignerTests
{
    private readonly ReportUrlSigner _signer;

    public ReportUrlSignerTests()
    {
        var options = Options.Create(new ReportSigningOptions
        {
            Secret = "test-secret-key-for-unit-tests",
            ExpiryMinutes = 15
        });
        _signer = new ReportUrlSigner(options);
    }

    [Fact]
    public void GenerateSignedUrl_ContainsSignatureParameter()
    {
        var url = _signer.GenerateSignedUrl("RPT001", "TMP001", "1HGCM82633A123456");

        Assert.Contains("signature=", url);
    }

    [Fact]
    public void GenerateSignedUrl_ContainsExpiryParameter()
    {
        var url = _signer.GenerateSignedUrl("RPT001", "TMP001", "1HGCM82633A123456");

        Assert.Contains("exp=", url);
    }

    [Fact]
    public void GenerateSignedUrl_SameInputsProduceSameSignature()
    {
        // Same inputs within the same second should produce the same signature
        var url1 = _signer.GenerateSignedUrl("RPT001", "TMP001", "1HGCM82633A123456");
        var url2 = _signer.GenerateSignedUrl("RPT001", "TMP001", "1HGCM82633A123456");

        var sig1 = ExtractQueryParam(url1, "signature");
        var sig2 = ExtractQueryParam(url2, "signature");

        Assert.Equal(sig1, sig2);
    }

    [Fact]
    public void GenerateSignedUrl_DifferentVinsProduceDifferentSignatures()
    {
        var url1 = _signer.GenerateSignedUrl("RPT001", "TMP001", "1HGCM82633A123456");
        var url2 = _signer.GenerateSignedUrl("RPT001", "TMP001", "2HGCM82633A654321");

        var sig1 = ExtractQueryParam(url1, "signature");
        var sig2 = ExtractQueryParam(url2, "signature");

        Assert.NotEqual(sig1, sig2);
    }

    [Fact]
    public void GenerateSignedUrl_DifferentTemplatesProduceDifferentSignatures()
    {
        var url1 = _signer.GenerateSignedUrl("RPT001", "TMP001", "1HGCM82633A123456");
        var url2 = _signer.GenerateSignedUrl("RPT001", "TMP002", "1HGCM82633A123456");

        var sig1 = ExtractQueryParam(url1, "signature");
        var sig2 = ExtractQueryParam(url2, "signature");

        Assert.NotEqual(sig1, sig2);
    }

    [Fact]
    public void GenerateSignedUrl_DifferentSecretProducesDifferentSignature()
    {
        var otherOptions = Options.Create(new ReportSigningOptions
        {
            Secret = "completely-different-secret",
            ExpiryMinutes = 15
        });
        var otherSigner = new ReportUrlSigner(otherOptions);

        var url1 = _signer.GenerateSignedUrl("RPT001", "TMP001", "1HGCM82633A123456");
        var url2 = otherSigner.GenerateSignedUrl("RPT001", "TMP001", "1HGCM82633A123456");

        var sig1 = ExtractQueryParam(url1, "signature");
        var sig2 = ExtractQueryParam(url2, "signature");

        Assert.NotEqual(sig1, sig2);
    }

    [Fact]
    public void GenerateSignedUrl_CustomExpiryChangesSignature()
    {
        var url1 = _signer.GenerateSignedUrl("RPT001", "TMP001", "1HGCM82633A123456", 15);
        var url2 = _signer.GenerateSignedUrl("RPT001", "TMP001", "1HGCM82633A123456", 60);

        var sig1 = ExtractQueryParam(url1, "signature");
        var sig2 = ExtractQueryParam(url2, "signature");

        // Different expiry → different signature
        Assert.NotEqual(sig1, sig2);
    }

    [Fact]
    public void GenerateSignedUrl_ExpiryIsInTheFuture()
    {
        var url = _signer.GenerateSignedUrl("RPT001", "TMP001", "1HGCM82633A123456");

        var expStr = ExtractQueryParam(url, "exp");
        Assert.True(long.TryParse(expStr, out var exp));

        var expiryTime = DateTimeOffset.FromUnixTimeSeconds(exp);
        Assert.True(expiryTime > DateTimeOffset.UtcNow, "Expiry should be in the future");
    }

    [Fact]
    public void GenerateSignedUrl_BaseUrlIsCorrect()
    {
        var url = _signer.GenerateSignedUrl("RPT001", "TMP001", "1HGCM82633A123456");

        Assert.StartsWith("/api/BuyersGuide/report/download?", url);
    }

    private static string ExtractQueryParam(string url, string paramName)
    {
        // URL is a relative path like /api/BuyersGuide/report/download?...
        var queryStart = url.IndexOf('?');
        if (queryStart < 0) return string.Empty;
        var query = System.Web.HttpUtility.ParseQueryString(url.Substring(queryStart));
        return query[paramName] ?? string.Empty;
    }
}
