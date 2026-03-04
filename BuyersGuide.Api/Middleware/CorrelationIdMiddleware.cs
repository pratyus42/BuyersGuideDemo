using BuyersGuide.Api.Constants;

namespace BuyersGuide.Api.Middleware;

/// <summary>
/// Middleware that reads or generates a correlation ID for every request.
/// The correlation ID is added to the response headers and injected into
/// the logging scope so all log entries for a request share it.
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Read incoming header or generate a new one
        if (!context.Request.Headers.TryGetValue(HeaderNames.CorrelationId, out var correlationId)
            || string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N");
        }

        // Store in HttpContext.Items so other middleware/services can access it
        context.Items["CorrelationId"] = correlationId.ToString();

        // Return on the response
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderNames.CorrelationId] = correlationId.ToString();
            return Task.CompletedTask;
        });

        // Push into log scope
        using (context.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger<CorrelationIdMiddleware>()
            .BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId.ToString()! }))
        {
            await _next(context);
        }
    }
}
