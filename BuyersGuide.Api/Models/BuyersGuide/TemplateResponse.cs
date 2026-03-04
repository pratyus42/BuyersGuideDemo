namespace BuyersGuide.Api.Models.BuyersGuide;

/// <summary>
/// Response payload for GET /api/BuyersGuide/templates.
/// Contains the dealer ID and the list of templates available to that dealer.
/// </summary>
public class TemplateResponse
{
    /// <summary>
    /// The dealer identifier.
    /// </summary>
    public string DealerId { get; set; } = string.Empty;

    /// <summary>
    /// Templates available to this dealer.
    /// </summary>
    public List<TemplateDto> Templates { get; set; } = new();
}
