using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BuyersGuide.Api.Swagger;

/// <summary>
/// Extension methods for configuring Swagger/OpenAPI documentation.
/// Documents required headers, endpoint descriptions, and example payloads.
/// </summary>
public static class SwaggerConfiguration
{
    public static IServiceCollection AddBuyersGuideSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "BuyersGuide API",
                Version = "v1",
                Description = "Mock Buyers Guide Report API with dealer-scoped template listing and report generation."
            });

            // Document X-Api-Key header
            options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Name = "X-Api-Key",
                Description = "API key for authentication (required)"
            });

            // Document X-Dealer-Id header
            options.AddSecurityDefinition("DealerId", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Name = "X-Dealer-Id",
                Description = "Dealer identifier for authorization context (required)"
            });

            // Apply security requirement globally
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "ApiKey"
                        }
                    },
                    Array.Empty<string>()
                },
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "DealerId"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}
