using System.Net;
using BuyersGuide.Api.Constants;
using Microsoft.AspNetCore.Mvc;

namespace BuyersGuide.Api.Authentication;

/// <summary>
/// Mock authentication middleware that validates X-Api-Key and extracts X-Dealer-Id.
/// In a production system this would be replaced with JWT/OAuth validation.
/// </summary>
public class MockAuthMiddleware
{
    private readonly RequestDelegate _next;

    public MockAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, DealerContext dealerContext)
    {
        // Skip auth for Swagger endpoints and report download (signature-validated separately)
        var path = context.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/api/BuyersGuide/report/download", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // Require X-Api-Key header
        if (!context.Request.Headers.TryGetValue(HeaderNames.ApiKey, out var apiKey)
            || string.IsNullOrWhiteSpace(apiKey))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                Title = "Unauthorized",
                Status = (int)HttpStatusCode.Unauthorized,
                Detail = "Missing or invalid X-Api-Key header.",
                Instance = context.Request.Path
            };

            await context.Response.WriteAsJsonAsync(problem);
            return;
        }

        // Extract X-Dealer-Id header
        if (!context.Request.Headers.TryGetValue(HeaderNames.DealerId, out var dealerId)
            || string.IsNullOrWhiteSpace(dealerId))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                Title = "Unauthorized",
                Status = (int)HttpStatusCode.Unauthorized,
                Detail = "Missing or invalid X-Dealer-Id header.",
                Instance = context.Request.Path
            };

            await context.Response.WriteAsJsonAsync(problem);
            return;
        }

        // Populate the DealerContext for the duration of this request
        dealerContext.DealerId = dealerId!;
        dealerContext.IsAuthenticated = true;

        await _next(context);
    }
}
