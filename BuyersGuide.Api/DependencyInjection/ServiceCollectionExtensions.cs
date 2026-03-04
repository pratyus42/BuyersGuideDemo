using BuyersGuide.Api.Authentication;
using BuyersGuide.Api.Configuration;
using BuyersGuide.Api.Helpers;
using BuyersGuide.Api.Services.Implementations;
using BuyersGuide.Api.Services.Interfaces;

namespace BuyersGuide.Api.DependencyInjection;

/// <summary>
/// Extension methods for registering application services into DI.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBuyersGuideServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind ReportSigning options from configuration
        services.Configure<ReportSigningOptions>(
            configuration.GetSection(ReportSigningOptions.SectionName));

        // Scoped: one DealerContext per request
        services.AddScoped<DealerContext>();

        // URL signing helper
        services.AddSingleton<ReportUrlSigner>();

        // Application services
        services.AddScoped<IBuyersGuideService, BuyersGuideService>();
        services.AddScoped<IReportApiClient, ReportApiClient>();

        return services;
    }
}
