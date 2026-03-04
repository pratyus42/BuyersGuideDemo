using System.Text;
using BuyersGuide.Api.Authentication;
using BuyersGuide.Api.Helpers;
using BuyersGuide.Api.Models.BuyersGuide;
using BuyersGuide.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BuyersGuide.Api.Controllers;

/// <summary>
/// Buyers Guide API endpoints for template listing and report generation.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BuyersGuideController : ControllerBase
{
    private readonly IBuyersGuideService _buyersGuideService;
    private readonly DealerContext _dealerContext;
    private readonly ReportUrlSigner _urlSigner;
    private readonly ILogger<BuyersGuideController> _logger;

    public BuyersGuideController(
        IBuyersGuideService buyersGuideService,
        DealerContext dealerContext,
        ReportUrlSigner urlSigner,
        ILogger<BuyersGuideController> logger)
    {
        _buyersGuideService = buyersGuideService;
        _dealerContext = dealerContext;
        _urlSigner = urlSigner;
        _logger = logger;
    }

    /// <summary>
    /// Returns templates available for a dealer.
    /// </summary>
    /// <param name="dealerId">Dealer identifier (must match X-Dealer-Id header).</param>
    /// <returns>Dealer-scoped template list.</returns>
    /// <response code="200">Templates retrieved successfully.</response>
    /// <response code="400">Missing or invalid dealerId parameter.</response>
    /// <response code="401">Authentication failed.</response>
    /// <response code="403">Dealer mismatch — dealerId does not match authenticated context.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpGet("templates")]
    [ProducesResponseType(typeof(TemplateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTemplates([FromQuery] string? dealerId)
    {
        // Validate dealerId query parameter
        if (string.IsNullOrWhiteSpace(dealerId))
        {
            return BadRequest(new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Bad Request",
                Status = StatusCodes.Status400BadRequest,
                Detail = "The dealerId query parameter is required.",
                Instance = HttpContext.Request.Path
            });
        }

        // Enforce dealer isolation: query dealerId MUST match authenticated X-Dealer-Id
        if (!string.Equals(dealerId, _dealerContext.DealerId, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Dealer isolation violation: query dealerId={QueryDealerId} vs authenticated={AuthDealerId}",
                dealerId, _dealerContext.DealerId);

            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                Title = "Forbidden",
                Status = StatusCodes.Status403Forbidden,
                Detail = "You are not authorized to access templates for this dealer.",
                Instance = HttpContext.Request.Path
            });
        }

        var response = await _buyersGuideService.GetTemplatesAsync(dealerId);
        return Ok(response);
    }

    /// <summary>
    /// Generates a Buyers Guide report for a VIN using the specified template.
    /// </summary>
    /// <param name="request">Report generation request body.</param>
    /// <returns>Report response containing a signed, time-bounded reportUrl.</returns>
    /// <response code="200">Report generated successfully.</response>
    /// <response code="400">Invalid request body.</response>
    /// <response code="401">Authentication failed.</response>
    /// <response code="403">Template not accessible for authenticated dealer.</response>
    /// <response code="422">Validation error (invalid templateId or vin).</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpPost("report")]
    [ProducesResponseType(typeof(ReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GenerateReport([FromBody] ReportRequest request)
    {
        // ModelState validation is automatic via [ApiController], but we remap to 422
        if (!ModelState.IsValid)
        {
            var validationProblem = new ValidationProblemDetails(ModelState)
            {
                Type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                Title = "Validation Error",
                Status = StatusCodes.Status422UnprocessableEntity,
                Detail = "One or more validation errors occurred.",
                Instance = HttpContext.Request.Path
            };
            return UnprocessableEntity(validationProblem);
        }

        var dealerId = _dealerContext.DealerId!;
        var result = await _buyersGuideService.GenerateReportAsync(dealerId, request);

        if (result is null)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                Title = "Forbidden",
                Status = StatusCodes.Status403Forbidden,
                Detail = $"Template '{request.TemplateId}' is not accessible for your dealer.",
                Instance = HttpContext.Request.Path
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// Downloads a generated Buyers Guide report.
    /// This endpoint is protected by HMAC signature validation (no auth headers needed).
    /// </summary>
    /// <param name="reportId">Report identifier.</param>
    /// <param name="templateId">Template used.</param>
    /// <param name="vin">Vehicle Identification Number.</param>
    /// <param name="exp">Unix timestamp expiry.</param>
    /// <param name="signature">HMAC-SHA256 signature.</param>
    /// <returns>An HTML Buyers Guide report.</returns>
    [HttpGet("report/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ApiExplorerSettings(IgnoreApi = true)] // Hide from Swagger — accessed via signed URL
    public IActionResult DownloadReport(
        [FromQuery] string reportId,
        [FromQuery] string templateId,
        [FromQuery] string vin,
        [FromQuery] long exp,
        [FromQuery] string signature)
    {
        // Validate signature and expiry
        if (!_urlSigner.ValidateSignature(reportId, templateId, vin, exp, signature))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                Title = "Forbidden",
                Status = StatusCodes.Status403Forbidden,
                Detail = "Invalid or expired report link.",
                Instance = HttpContext.Request.Path
            });
        }

        var expiryTime = DateTimeOffset.FromUnixTimeSeconds(exp);
        var generatedAt = expiryTime.AddMinutes(-15); // approximate generation time

        // Generate a sample HTML Buyers Guide report
        var html = GenerateReportHtml(reportId, templateId, vin, generatedAt);
        var bytes = Encoding.UTF8.GetBytes(html);

        return File(bytes, "text/html", $"BuyersGuide_{vin}_{templateId}.html");
    }

    private static string GenerateReportHtml(string reportId, string templateId, string vin, DateTimeOffset generatedAt)
    {
        var templateName = templateId switch
        {
            "TMP001" => "Standard Buyers Guide",
            "TMP002" => "Warranty Buyers Guide",
            "TMP003" => "Premium Buyers Guide",
            _ => "Buyers Guide"
        };

        return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <title>Buyers Guide - {vin}</title>
    <style>
        body {{ font-family: Arial, sans-serif; max-width: 800px; margin: 40px auto; padding: 20px; color: #333; }}
        .header {{ text-align: center; border-bottom: 3px solid #1a5276; padding-bottom: 20px; margin-bottom: 30px; }}
        .header h1 {{ color: #1a5276; margin-bottom: 5px; }}
        .header h2 {{ color: #2e86c1; font-weight: normal; }}
        .section {{ margin-bottom: 25px; padding: 15px; background: #f8f9fa; border-left: 4px solid #2e86c1; }}
        .section h3 {{ color: #1a5276; margin-top: 0; }}
        table {{ width: 100%; border-collapse: collapse; margin: 10px 0; }}
        th, td {{ padding: 10px 12px; text-align: left; border-bottom: 1px solid #ddd; }}
        th {{ background: #1a5276; color: white; }}
        .warranty-box {{ border: 2px solid #27ae60; padding: 15px; margin: 15px 0; background: #eafaf1; }}
        .warranty-box h3 {{ color: #27ae60; margin-top: 0; }}
        .as-is-box {{ border: 2px solid #e74c3c; padding: 15px; margin: 15px 0; background: #fdedec; }}
        .as-is-box h3 {{ color: #e74c3c; margin-top: 0; }}
        .footer {{ text-align: center; margin-top: 40px; padding-top: 20px; border-top: 2px solid #ddd; font-size: 12px; color: #888; }}
        .badge {{ display: inline-block; padding: 5px 15px; border-radius: 4px; font-weight: bold; color: white; }}
        .badge-standard {{ background: #2e86c1; }}
        .badge-warranty {{ background: #27ae60; }}
        .badge-premium {{ background: #8e44ad; }}
    </style>
</head>
<body>
    <div class=""header"">
        <h1>BUYERS GUIDE</h1>
        <h2>{templateName}</h2>
        <span class=""badge {(templateId == "TMP001" ? "badge-standard" : templateId == "TMP002" ? "badge-warranty" : "badge-premium")}"">{templateName}</span>
    </div>

    <div class=""section"">
        <h3>Vehicle Information</h3>
        <table>
            <tr><th>Field</th><th>Value</th></tr>
            <tr><td>VIN</td><td><strong>{vin}</strong></td></tr>
            <tr><td>Report ID</td><td>{reportId}</td></tr>
            <tr><td>Template</td><td>{templateId} — {templateName}</td></tr>
            <tr><td>Generated</td><td>{generatedAt:yyyy-MM-dd HH:mm:ss} UTC</td></tr>
        </table>
    </div>

    {(templateId == "TMP002" || templateId == "TMP003" ? @"
    <div class=""warranty-box"">
        <h3>&#9989; WARRANTY</h3>
        <p><strong>The dealer will repair or replace covered systems that fail during the warranty period.</strong></p>
        <table>
            <tr><th>System Covered</th><th>Duration</th></tr>
            <tr><td>Engine</td><td>30 days or 1,000 miles</td></tr>
            <tr><td>Transmission</td><td>30 days or 1,000 miles</td></tr>
            <tr><td>Drive Axle</td><td>30 days or 1,000 miles</td></tr>
        </table>
        <p><em>Ask the dealer for a copy of the warranty document for a full explanation of warranty coverage, exclusions, and the dealer's repair obligations.</em></p>
    </div>
    " : @"
    <div class=""as-is-box"">
        <h3>&#10060; AS IS — NO DEALER WARRANTY</h3>
        <p><strong>The dealer does not provide a warranty for any repairs after sale.</strong></p>
        <p>You will pay all costs for any repairs. The dealer assumes no responsibility for any repairs regardless of any oral statements about the vehicle.</p>
    </div>
    ")}

    <div class=""section"">
        <h3>Important Disclosures</h3>
        <ul>
            <li>You may inspect this vehicle prior to purchase.</li>
            <li>You may request an independent inspection by a mechanic.</li>
            <li>This Buyers Guide form is required by the FTC's Used Car Rule.</li>
            <li>Refer to the FTC at <strong>www.ftc.gov</strong> for more information.</li>
        </ul>
    </div>

    <div class=""section"">
        <h3>Systems Covered</h3>
        <table>
            <tr><th>System</th><th>Status</th></tr>
            <tr><td>Engine</td><td>Inspected</td></tr>
            <tr><td>Transmission</td><td>Inspected</td></tr>
            <tr><td>Rear Axle / Drive Axle</td><td>Inspected</td></tr>
            <tr><td>Steering</td><td>Inspected</td></tr>
            <tr><td>Suspension</td><td>Inspected</td></tr>
            <tr><td>Brakes</td><td>Inspected</td></tr>
            <tr><td>Electrical</td><td>Inspected</td></tr>
            <tr><td>Air Conditioning</td><td>Inspected</td></tr>
        </table>
    </div>

    <div class=""footer"">
        <p>Report ID: {reportId} | Generated: {generatedAt:yyyy-MM-dd HH:mm:ss} UTC</p>
        <p>This is a mock Buyers Guide report generated for demonstration purposes.</p>
    </div>
</body>
</html>";
    }
}
