# Quickstart: Buyers Guide Report API

**Feature**: [specs/001-buyersguide-api/spec.md](spec.md)
**Date**: 2026-03-04

This quickstart describes how to run and test the BuyersGuide API once implemented.

## Prerequisites

- .NET SDK (latest LTS; .NET 8 preferred)

## Run

From the repository root:

- `dotnet restore`
- `dotnet run --project BuyersGuide.Api`

## Swagger

Open:

- `https://localhost:{port}/swagger`

## Example requests

### 1) GET templates

```bash
curl -k "https://localhost:{port}/api/BuyersGuide/templates?dealerId=D1001" \
  -H "X-Api-Key: dev" \
  -H "X-Dealer-Id: D1001" \
  -H "X-Correlation-Id: demo-001"
```

### 2) POST report

```bash
curl -k "https://localhost:{port}/api/BuyersGuide/report" \
  -H "Content-Type: application/json" \
  -H "X-Api-Key: dev" \
  -H "X-Dealer-Id: D1001" \
  -d "{\"templateId\":\"TMP001\",\"vin\":\"1HGCM82633A123456\"}"
```

Expected: `reportUrl` is signed and time-bounded. The URL is treated as sensitive and MUST NOT appear in logs.
