using System.Diagnostics;

namespace BuyersGuide.Api.Middleware;

/// <summary>
/// Request logging middleware that logs method, path, status code, and elapsed time.
/// IMPORTANT: Must NOT log response bodies or reportUrl (signed URLs are secrets).
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Items.TryGetValue("CorrelationId", out var cid)
            ? cid?.ToString() ?? "-"
            : "-";

        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "[{CorrelationId}] → {Method} {Path}",
            correlationId, context.Request.Method, context.Request.Path);

        await _next(context);

        stopwatch.Stop();

        _logger.LogInformation(
            "[{CorrelationId}] ← {Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
            correlationId,
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds);
    }
}
