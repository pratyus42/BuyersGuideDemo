# Research: Buyers Guide Report API

**Feature**: [specs/001-buyersguide-api/spec.md](spec.md)
**Date**: 2026-03-04

This document captures key technical decisions required to implement the feature while meeting the constitution.

## Decision 1: API style (Controllers vs. Minimal APIs)

- Decision: Use ASP.NET Core Controllers (attribute routing).
- Rationale: Required folder structure includes `Controllers/BuyersGuideController.cs` and aligns with auto-validation via `[ApiController]`.
- Alternatives considered:
  - Minimal APIs: simpler for small services but does not match the required structure.

## Decision 2: Validation strategy

- Decision: Use DataAnnotations + `[ApiController]` model validation for request bodies and explicit validation for query parameters.
- Rationale: Keeps dependencies minimal and provides predictable 400 responses for invalid models.
- Alternatives considered:
  - FluentValidation: stronger expressiveness, but adds a dependency not required for this mock service.

## Decision 3: Error response shape

- Decision: Standardize on RFC 7807 Problem Details JSON for errors.
- Rationale: Works well with ASP.NET Core, is widely supported, and satisfies the constitution requirement for a consistent error shape.
- Alternatives considered:
  - Custom `{ errorCode, message }` DTO: workable, but Problem Details is more standard.

## Decision 4: Authentication + dealer authorization (mocked)

- Decision: Use a simple header-based authentication mechanism suitable for mock mode:
  - `X-Api-Key`: required
  - `X-Dealer-Id`: represents the authenticated dealer context
- Rationale: Meets the constitution’s AuthN/AuthZ and dealer isolation constraints without external identity provider dependencies.
- Alternatives considered:
  - JWT bearer authentication: realistic, but out of scope for a mocked reference implementation.

## Decision 5: Correlation ID

- Decision: Middleware that:
  - Accepts `X-Correlation-Id` if provided; otherwise generates one
  - Returns it on responses and injects it into logging scope
- Rationale: Improves operability and supports tracing in logs.
- Alternatives considered:
  - Using only built-in `TraceIdentifier`: fine, but explicit header improves cross-service traceability.

## Decision 6: Signed report URL

- Decision: `ReportApiClient` generates a signed, time-bounded URL:
  - Format: `https://mockreports.service.com/buyersguide/{reportId}?exp={unixSeconds}&sig={signature}`
  - Signature: HMAC-SHA256 over `{reportId}.{templateId}.{vin}.{exp}`
- Rationale: Satisfies “signed + time-bounded” while keeping the service self-contained.
- Alternatives considered:
  - Unsigned URL: violates constitution.
  - Hosting a real downloadable PDF inside this API: would introduce additional download route(s), conflicting with the “two endpoints” constraint.

## Open Questions

- None blocking for planning. Any organization-specific auth policy can replace the mock headers without changing the endpoint shapes.
