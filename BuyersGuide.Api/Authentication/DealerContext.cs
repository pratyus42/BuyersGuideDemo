namespace BuyersGuide.Api.Authentication;

/// <summary>
/// Scoped service that holds the authenticated dealer context for the current request.
/// </summary>
public class DealerContext
{
    /// <summary>
    /// The dealer ID extracted from the authenticated request headers.
    /// </summary>
    public string? DealerId { get; set; }

    /// <summary>
    /// Whether the current request has been authenticated.
    /// </summary>
    public bool IsAuthenticated { get; set; }
}
