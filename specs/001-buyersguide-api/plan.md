# Implementation Plan: Buyers Guide Report API

**Branch**: `001-buyersguide-api` | **Date**: 2026-03-04 | **Spec**: [specs/001-buyersguide-api/spec.md](spec.md)
**Input**: Feature specification from [specs/001-buyersguide-api/spec.md](spec.md)

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Implement a mock BuyersGuide REST API with two endpoints:

- `GET /api/BuyersGuide/templates` returns dealer-scoped template metadata (`templateId`, `templateName`, `printableTemplateId`).
- `POST /api/BuyersGuide/report` generates a BuyersGuide report for a VIN using a selected template and returns a signed, time-bounded `reportUrl`.

Technical approach (per [specs/001-buyersguide-api/research.md](research.md)):

- ASP.NET Core Controllers
- Problem Details error responses
- Header-based mock authentication + dealer authorization
- Correlation ID + structured logging
- HMAC-signed report URLs

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: C# + .NET 8 (ASP.NET Core)  
**Primary Dependencies**: ASP.NET Core MVC, Swashbuckle (OpenAPI/Swagger)  
**Storage**: N/A (in-memory mock data)  
**Testing**: xUnit + ASP.NET Core test host (Microsoft.AspNetCore.Mvc.Testing)  
**Target Platform**: Windows/Linux server  
**Project Type**: web-service (REST API)  
**Performance Goals**: p95 < 1s for template listing; p95 < 5s for report generation  
**Constraints**: No logging of signed URLs; dealer isolation; consistent error shape (Problem Details); correlation id on every request/response  
**Scale/Scope**: Mock service for developer/testing workflows (no external dependencies)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- Scope stays within the two BuyersGuide endpoints unless the constitution is amended.
- Contract is updated for any API surface change (inputs/outputs/status codes).
- AuthN/AuthZ and dealer isolation are explicitly addressed in the design.
- Error shape/status codes are explicitly defined (no ambiguous failures).
- Signed report URLs are treated as secrets (no logging, time-bounded).
- Operability: structured logs include correlation id + outcome, without sensitive data.

**Post-design re-check (PASS)**:

- Contract is captured in [specs/001-buyersguide-api/contracts/http-api.md](contracts/http-api.md) and defines exactly two endpoints.
- Dealer isolation rules are explicit for both endpoints.
- Error shape is standardized (Problem Details) and avoids leaking secrets.
- Signed `reportUrl` is time-bounded and treated as sensitive (no logging).

## Project Structure

### Documentation (this feature)

```text
specs/001-buyersguide-api/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->

```text
BuyersGuide.Api/
├── Authentication/
├── Configuration/
├── Constants/
├── Controllers/
│   └── BuyersGuideController.cs
├── DependencyInjection/
│   └── ServiceCollectionExtensions.cs
├── Extensions/
├── Helpers/
├── Logging/
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs
├── Models/
│   └── BuyersGuide/
│       ├── TemplateDto.cs
│       ├── TemplateResponse.cs
│       ├── ReportRequest.cs
│       └── ReportResponse.cs
├── Services/
│   ├── Interfaces/
│   │   ├── IBuyersGuideService.cs
│   │   └── IReportApiClient.cs
│   └── Implementations/
│       ├── BuyersGuideService.cs
│       └── ReportApiClient.cs
├── Swagger/
│   └── SwaggerConfiguration.cs
├── Properties/
└── Program.cs

BuyersGuide.Api.Tests/
└── (unit + minimal integration tests for controller/service)
```

**Structure Decision**: Single API project + a test project, using the required `BuyersGuide.Api` folder layout.

## Execution Plan (Phased)

### Phase 1: Skeleton + cross-cutting concerns

- Create the `BuyersGuide.Api` project.
- Add controllers wiring, DI extension, and Swagger configuration.
- Add middleware: exception handling + request logging + correlation id.

### Phase 2: Models + contracts

- Implement the DTOs from [specs/001-buyersguide-api/data-model.md](data-model.md).
- Ensure OpenAPI shows example requests/responses matching [specs/001-buyersguide-api/contracts/http-api.md](contracts/http-api.md).

### Phase 3: Mock infrastructure + services

- In-memory template mappings per spec assumptions (D1001 → TMP001/TMP002, D2001 → TMP003).
- `ReportApiClient` generates `reportUrl` using a time-bounded HMAC signature.
- `BuyersGuideService` orchestrates validation/authorization and report generation.

### Phase 4: Endpoint implementation

- `GET /api/BuyersGuide/templates` enforces dealer match between query `dealerId` and authenticated `X-Dealer-Id`.
- `POST /api/BuyersGuide/report` validates inputs and ensures template belongs to dealer.

### Phase 5: Automated checks

- Add at least one automated check for the API contract (smoke/integration test) as required by the constitution.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
