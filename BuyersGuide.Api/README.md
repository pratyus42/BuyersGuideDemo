# BuyersGuide API

Mock Buyers Guide Report API built with ASP.NET Core (.NET 8).

## Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/BuyersGuide/templates?dealerId={id}` | List templates available for a dealer |
| POST | `/api/BuyersGuide/report` | Generate a signed report URL for a VIN |

## Running

```bash
dotnet restore
dotnet run --project BuyersGuide.Api
```

Swagger UI: `https://localhost:{port}/swagger`

## Authentication

All requests require these headers:

| Header | Description |
|--------|-------------|
| `X-Api-Key` | API key (any non-empty value in dev) |
| `X-Dealer-Id` | Dealer identifier for authorization context |

Optional:

| Header | Description |
|--------|-------------|
| `X-Correlation-Id` | Correlation ID for request tracing (auto-generated if missing) |

## Example Requests

### List templates

```bash
curl -k "https://localhost:5001/api/BuyersGuide/templates?dealerId=D1001" \
  -H "X-Api-Key: dev" \
  -H "X-Dealer-Id: D1001"
```

### Generate report

```bash
curl -k "https://localhost:5001/api/BuyersGuide/report" \
  -H "Content-Type: application/json" \
  -H "X-Api-Key: dev" \
  -H "X-Dealer-Id: D1001" \
  -d '{"templateId":"TMP001","vin":"1HGCM82633A123456"}'
```

## Mock Data

| Dealer | Templates |
|--------|-----------|
| D1001 | TMP001 (Standard Buyers Guide), TMP002 (Warranty Buyers Guide) |
| D2001 | TMP003 (Premium Buyers Guide) |

## Error Responses

All errors are returned as [RFC 7807 Problem Details](https://tools.ietf.org/html/rfc7807) JSON.

| Status | Meaning |
|--------|---------|
| 400 | Missing or invalid parameters |
| 401 | Missing authentication headers |
| 403 | Dealer mismatch or template not accessible |
| 422 | Validation error (invalid VIN format, etc.) |
| 500 | Internal server error |

## Testing

```bash
dotnet test
```

## Security Notes

- `reportUrl` values are signed with HMAC-SHA256 and time-bounded
- Report URLs are treated as secrets and MUST NOT be logged
- Dealer isolation is enforced on all endpoints
