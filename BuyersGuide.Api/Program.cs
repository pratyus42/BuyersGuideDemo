using BuyersGuide.Api.Authentication;
using BuyersGuide.Api.DependencyInjection;
using BuyersGuide.Api.Middleware;
using BuyersGuide.Api.Swagger;

var builder = WebApplication.CreateBuilder(args);

// Add controllers with custom validation behavior (return 422 instead of 400 for model validation)
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

// Add Swagger/OpenAPI (T006)
builder.Services.AddBuyersGuideSwagger();

// Register application services (T016)
builder.Services.AddBuyersGuideServices(builder.Configuration);

var app = builder.Build();

// Middleware pipeline ordering (T010):
// 1. Correlation ID
app.UseMiddleware<CorrelationIdMiddleware>();

// 2. Request logging
app.UseMiddleware<RequestLoggingMiddleware>();

// 3. Exception handling
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 4. Swagger (before routing so it's accessible)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BuyersGuide API v1");
        c.RoutePrefix = "swagger";
    });
}

// 5. Routing
app.UseRouting();

// 6. Authentication middleware
app.UseMiddleware<MockAuthMiddleware>();

// 7. Map controllers
app.MapControllers();

app.Run();

// Make the implicit Program class public so the test project can reference it
public partial class Program { }
