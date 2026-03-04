# BuyersGuide API

A RESTful API for generating vehicle Buyers Guide reports, built with **ASP.NET Core (.NET 8)**. The API allows dealers to list available report templates and generate signed, time-bounded report URLs for specific vehicles by VIN.

---

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [API Endpoints](#api-endpoints)
- [Authentication](#authentication)
- [Request & Response Models](#request--response-models)
- [Error Handling](#error-handling)
- [Mock Data](#mock-data)
- [Configuration](#configuration)
- [Testing](#testing)
- [Project Structure](#project-structure)
- [Security](#security)

---

## Overview

The BuyersGuide API provides two core capabilities:

1. **Template Listing** — Retrieve the report templates available to a specific dealer.
2. **Report Generation** — Generate a signed, time-bounded Buyers Guide report URL for a given vehicle (by VIN) using a specified template.

All endpoints enforce **dealer isolation**, ensuring that a dealer can only access their own templates and reports.

---

## Architecture

```
┌──────────────┐     ┌──────────────────────────┐     ┌─────────────────┐
│   Client     │────▶│   BuyersGuide API        │────▶│ Report Service  │
│  (Dealer)    │◀────│   (ASP.NET Core 8)       │◀────│   (Mock)        │
└──────────────┘     └──────────────────────────┘     └─────────────────┘
                              │
                     Middleware Pipeline:
                     1. Correlation ID
                     2. Request Logging
                     3. Exception Handling
                     4. Swagger (Dev only)
                     5. Routing
                     6. Mock Authentication
                     7. Controllers
```

**Key components:**

| Layer | Description |
|-------|-------------|
| **Controllers** | `BuyersGuideController` — handles HTTP requests for templates and reports |
| **Services** | `BuyersGuideService` — core business logic for template lookup and report generation |
| **Clients** | `ReportApiClient` — mock external report service client |
| **Middleware** | Correlation ID injection, structured request logging, global exception handling |
| **Authentication** | `MockAuthMiddleware` — validates `X-Api-Key` and `X-Dealer-Id` headers |
| **Helpers** | `ReportUrlSigner` — HMAC-SHA256 URL signing with time-bounded expiry |

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- A code editor (e.g., Visual Studio, VS Code, Rider)

---

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/pratyus42/BuyersGuideDemo.git
cd BuyersGuideDemo
```

### 2. Restore dependencies

```bash
dotnet restore
```

### 3. Build the solution

```bash
dotnet build
```

### 4. Run the API

```bash
dotnet run --project BuyersGuide.Api
```

The API will start on `https://localhost:5001` (or the port configured in `launchSettings.json`).

### 5. Open Swagger UI

Navigate to `https://localhost:{port}/swagger` in your browser to explore the API interactively.

---

## API Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/BuyersGuide/templates?dealerId={id}` | List templates available for a dealer |
| `POST` | `/api/BuyersGuide/report` | Generate a signed report URL for a VIN |

### GET `/api/BuyersGuide/templates`

Retrieves the list of report templates available to the specified dealer.

**Query Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `dealerId` | string | Yes | Dealer identifier (must match `X-Dealer-Id` header) |

**Example:**

```bash
curl -k "https://localhost:5001/api/BuyersGuide/templates?dealerId=D1001" \
  -H "X-Api-Key: dev" \
  -H "X-Dealer-Id: D1001"
```

**Success Response (200):**

```json
{
  "dealerId": "D1001",
  "templates": [
    {
      "templateId": "TMP001",
      "name": "Standard Buyers Guide"
    },
    {
      "templateId": "TMP002",
      "name": "Warranty Buyers Guide"
    }
  ]
}
```

### POST `/api/BuyersGuide/report`

Generates a Buyers Guide report for a specific vehicle using a template.

**Request Body:**

```json
{
  "templateId": "TMP001",
  "vin": "1HGCM82633A123456"
}
```

**Example:**

```bash
curl -k "https://localhost:5001/api/BuyersGuide/report" \
  -H "Content-Type: application/json" \
  -H "X-Api-Key: dev" \
  -H "X-Dealer-Id: D1001" \
  -d '{"templateId":"TMP001","vin":"1HGCM82633A123456"}'
```

**Success Response (200):**

```json
{
  "vin": "1HGCM82633A123456",
  "templateId": "TMP001",
  "reportUrl": "https://reports.example.com/report/...?sig=...&exp=..."
}
```

---

## Authentication

All requests require the following headers:

| Header | Required | Description |
|--------|----------|-------------|
| `X-Api-Key` | Yes | API key (any non-empty value accepted in development) |
| `X-Dealer-Id` | Yes | Dealer identifier for authorization context |
| `X-Correlation-Id` | No | Correlation ID for distributed tracing (auto-generated if not provided) |

> **Note:** In development mode, `MockAuthMiddleware` is used. Any non-empty `X-Api-Key` value is accepted.

---

## Request & Response Models

### `ReportRequest`

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `templateId` | string | Yes | Must be a valid template ID accessible to the dealer |
| `vin` | string | Yes | Exactly 17 alphanumeric characters; excludes I, O, Q |

### `ReportResponse`

| Field | Type | Description |
|-------|------|-------------|
| `vin` | string | The VIN used for report generation |
| `templateId` | string | The template used for report generation |
| `reportUrl` | string | Signed, time-bounded URL for downloading the report |

### `TemplateResponse`

| Field | Type | Description |
|-------|------|-------------|
| `dealerId` | string | The dealer identifier |
| `templates` | `TemplateDto[]` | List of templates available to this dealer |

---

## Error Handling

All errors are returned as [RFC 7807 Problem Details](https://tools.ietf.org/html/rfc7807) JSON objects.

| Status Code | Meaning |
|-------------|---------|
| `400` | Missing or invalid parameters |
| `401` | Missing authentication headers (`X-Api-Key` or `X-Dealer-Id`) |
| `403` | Dealer mismatch or template not accessible for the authenticated dealer |
| `422` | Validation error (e.g., invalid VIN format, missing required fields) |
| `500` | Internal server error |

**Example error response:**

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "The dealerId query parameter is required.",
  "instance": "/api/BuyersGuide/templates"
}
```

---

## Mock Data

The following dealer/template mappings are available in development:

| Dealer ID | Template ID | Template Name |
|-----------|-------------|---------------|
| `D1001` | `TMP001` | Standard Buyers Guide |
| `D1001` | `TMP002` | Warranty Buyers Guide |
| `D2001` | `TMP003` | Premium Buyers Guide |

---

## Configuration

Configuration is managed via `appsettings.json`:

```json
{
  "ReportSigning": {
    "Secret": "<HMAC-SHA256 signing key>",
    "ExpiryMinutes": 15
  }
}
```

| Setting | Description | Default |
|---------|-------------|---------|
| `ReportSigning:Secret` | HMAC-SHA256 secret key for signing report URLs | *(empty — must be configured)* |
| `ReportSigning:ExpiryMinutes` | Report URL expiration time in minutes | `15` |

---

## Testing

The project includes integration tests using **xUnit** and `Microsoft.AspNetCore.Mvc.Testing`.

### Run all tests

```bash
dotnet test
```

### Test categories

| Test File | Coverage |
|-----------|----------|
| `ReportEndpointTests` | Report generation endpoint — success and error scenarios |
| `ReportEndpointValidationTests` | Request validation (VIN format, required fields) |
| `ReportUrlSignerTests` | HMAC-SHA256 URL signing and expiry logic |
| `TemplatesEndpointTests` | Template listing endpoint — success and error scenarios |

---

## Project Structure

```
BuyersGuideDemo/
├── BuyersGuide.sln                         # Solution file
├── README.md                               # This file
├── BuyersGuide.Api/                        # Main API project
│   ├── Program.cs                          # Application entry point & middleware pipeline
│   ├── Authentication/                     # Auth middleware & dealer context
│   │   ├── DealerContext.cs
│   │   └── MockAuthMiddleware.cs
│   ├── Configuration/                      # Configuration option classes
│   │   └── ReportSigningOptions.cs
│   ├── Constants/                          # Constant values (header names, etc.)
│   │   └── HeaderNames.cs
│   ├── Controllers/                        # API controllers
│   │   └── BuyersGuideController.cs
│   ├── DependencyInjection/                # Service registration extensions
│   │   └── ServiceCollectionExtensions.cs
│   ├── Helpers/                            # Utility classes
│   │   └── ReportUrlSigner.cs
│   ├── Middleware/                          # Custom middleware
│   │   ├── CorrelationIdMiddleware.cs
│   │   ├── ExceptionHandlingMiddleware.cs
│   │   └── RequestLoggingMiddleware.cs
│   ├── Models/                             # Request/response DTOs
│   │   └── BuyersGuide/
│   │       ├── ReportRequest.cs
│   │       ├── ReportResponse.cs
│   │       ├── TemplateDto.cs
│   │       └── TemplateResponse.cs
│   ├── Services/                           # Business logic & external clients
│   │   ├── Implementations/
│   │   │   ├── BuyersGuideService.cs
│   │   │   └── ReportApiClient.cs
│   │   └── Interfaces/
│   │       ├── IBuyersGuideService.cs
│   │       └── IReportApiClient.cs
│   └── Swagger/                            # Swagger/OpenAPI configuration
│       └── SwaggerConfiguration.cs
├── BuyersGuide.Api.Tests/                  # Integration & unit tests
│   ├── ReportEndpointTests.cs
│   ├── ReportEndpointValidationTests.cs
│   ├── ReportUrlSignerTests.cs
│   └── TemplatesEndpointTests.cs
└── specs/                                  # Design specifications & contracts
    └── 001-buyersguide-api/
        ├── spec.md
        ├── plan.md
        ├── tasks.md
        └── contracts/
            └── http-api.md
```

---

## Security

- **Report URL Signing** — Report URLs are signed with HMAC-SHA256 and include a time-bounded expiry to prevent unauthorized access.
- **Sensitive Data** — `reportUrl` values are treated as secrets and are **never logged**.
- **Dealer Isolation** — All endpoints enforce that the authenticated dealer can only access their own data.
- **Correlation IDs** — Every request is tagged with a correlation ID for distributed tracing and debugging.

---

## License

*This project is for internal/demo use.*
