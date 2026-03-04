---

description: "Task list for Buyers Guide Report API implementation"
---

# Tasks: Buyers Guide Report API

**Input**: Design documents from [specs/001-buyersguide-api/](.)
**Prerequisites**: [specs/001-buyersguide-api/plan.md](plan.md), [specs/001-buyersguide-api/spec.md](spec.md), [specs/001-buyersguide-api/contracts/http-api.md](contracts/http-api.md), [specs/001-buyersguide-api/data-model.md](data-model.md)

**Tests**: Tests are OPTIONAL unless required by the constitution/plan. This feature MUST include at least one automated check that validates the HTTP contract at a minimum (constitution quality gate).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `- [ ] T### [P?] [US#?] Description with file path`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[US#]**: Which user story this task belongs to (US1, US2, US3). Not used for Setup/Foundational/Polish phases.
- Every task includes an exact file path.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the solution + API project skeleton with the required folder layout.

- [x] T001 Create solution file `BuyersGuide.sln` at repo root
- [x] T002 Create ASP.NET Core Web API project `BuyersGuide.Api/BuyersGuide.Api.csproj` (target .NET 8)
- [x] T003 Create test project `BuyersGuide.Api.Tests/BuyersGuide.Api.Tests.csproj` and reference `BuyersGuide.Api/BuyersGuide.Api.csproj`
- [x] T004 [P] Create required folder structure under `BuyersGuide.Api/` (Authentication, Configuration, Constants, Controllers, DependencyInjection, Extensions, Helpers, Logging, Middleware, Models/BuyersGuide, Services/Interfaces, Services/Implementations, Swagger)
- [x] T005 Wire the projects into `BuyersGuide.sln` (add `BuyersGuide.Api/BuyersGuide.Api.csproj` and `BuyersGuide.Api.Tests/BuyersGuide.Api.Tests.csproj`)

**Checkpoint**: `dotnet build` succeeds and the project layout matches plan.md.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Cross-cutting concerns required before implementing any endpoint logic.

- [x] T006 Add Swagger/OpenAPI package + baseline setup in `BuyersGuide.Api/Program.cs`
- [x] T007 Implement centralized exception handling returning Problem Details in `BuyersGuide.Api/Middleware/ExceptionHandlingMiddleware.cs`
- [x] T008 Implement correlation id middleware in `BuyersGuide.Api/Middleware/CorrelationIdMiddleware.cs` (read/write `X-Correlation-Id`)
- [x] T009 Implement request logging middleware in `BuyersGuide.Api/Middleware/RequestLoggingMiddleware.cs` (log method/path/status + correlation id; DO NOT log `reportUrl`)
- [x] T010 Add middleware pipeline ordering in `BuyersGuide.Api/Program.cs` (correlation → request logging → exception handling → routing → auth → controllers)

- [x] T011 Define header constants in `BuyersGuide.Api/Constants/HeaderNames.cs` (`X-Api-Key`, `X-Dealer-Id`, `X-Correlation-Id`)
- [x] T012 Implement mock authentication/authorization middleware in `BuyersGuide.Api/Authentication/MockAuthMiddleware.cs` (require `X-Api-Key`; extract `X-Dealer-Id` as dealer context)
- [x] T013 Define dealer context accessor in `BuyersGuide.Api/Authentication/DealerContext.cs` (stores `DealerId` for the request)
- [x] T014 Add auth middleware registration in `BuyersGuide.Api/Program.cs` (ensure unauthenticated requests return 401 Problem Details)

- [x] T015 Add Swagger configuration helper in `BuyersGuide.Api/Swagger/SwaggerConfiguration.cs` (enable examples and document required headers)
- [x] T016 Add service registration extension in `BuyersGuide.Api/DependencyInjection/ServiceCollectionExtensions.cs` (register BuyersGuide service + ReportApi client)

**Checkpoint**: API runs and `/swagger` loads with both routes planned (even if not implemented yet).

---

## Phase 3: User Story 1 - List templates for a dealer (Priority: P1) 🎯 MVP

**Goal**: Return dealer-scoped template metadata: `templateId`, `templateName`, `printableTemplateId`.

**Independent Test**: Call `GET /api/BuyersGuide/templates?dealerId=D1001` with headers `X-Api-Key` and `X-Dealer-Id: D1001` and receive the expected template list.

### Implementation (US1)

- [x] T017 [P] [US1] Implement DTO `BuyersGuide.Api/Models/BuyersGuide/TemplateDto.cs`
- [x] T018 [P] [US1] Implement DTO `BuyersGuide.Api/Models/BuyersGuide/TemplateResponse.cs`

- [x] T019 [P] [US1] Create service contract `BuyersGuide.Api/Services/Interfaces/IBuyersGuideService.cs` (`GetTemplatesAsync(string dealerId)`)
- [x] T020 [US1] Implement `GetTemplatesAsync` in `BuyersGuide.Api/Services/Implementations/BuyersGuideService.cs` with in-memory dealer→templates mapping

- [x] T021 [US1] Implement controller action in `BuyersGuide.Api/Controllers/BuyersGuideController.cs` for `GET /api/BuyersGuide/templates`
- [x] T022 [US1] Enforce dealer isolation in `BuyersGuide.Api/Controllers/BuyersGuideController.cs` (query `dealerId` MUST match authenticated `X-Dealer-Id`; else 403)
- [x] T023 [US1] Ensure Swagger documents the endpoint, query param, and required headers in `BuyersGuide.Api/Swagger/SwaggerConfiguration.cs`

### Automated check (required by constitution)

- [x] T024 [US1] Add integration test for templates endpoint in `BuyersGuide.Api.Tests/TemplatesEndpointTests.cs`

**Checkpoint**: US1 passes its independent test and the integration test is green.

---

## Phase 4: User Story 2 - Generate a Buyers Guide report (Priority: P2)

**Goal**: Generate a report URL for a VIN using the selected template.

**Independent Test**: Call `POST /api/BuyersGuide/report` with `{ "templateId": "TMP001", "vin": "1HGCM82633A123456" }` and headers `X-Api-Key`, `X-Dealer-Id: D1001`, and receive a response containing `reportUrl`.

### Implementation (US2)

- [x] T025 [P] [US2] Implement request model `BuyersGuide.Api/Models/BuyersGuide/ReportRequest.cs` with DataAnnotations validation
- [x] T026 [P] [US2] Implement response model `BuyersGuide.Api/Models/BuyersGuide/ReportResponse.cs`

- [x] T027 [P] [US2] Create client contract `BuyersGuide.Api/Services/Interfaces/IReportApiClient.cs` (`GenerateReportAsync(string templateId, string vin)`)
- [x] T028 [P] [US2] Add signing options model `BuyersGuide.Api/Configuration/ReportSigningOptions.cs` (secret + expiry minutes)
- [x] T029 [P] [US2] Implement URL signing helper in `BuyersGuide.Api/Helpers/ReportUrlSigner.cs` (HMAC signature; include expiry)
- [x] T030 [US2] Implement `GenerateReportAsync` in `BuyersGuide.Api/Services/Implementations/ReportApiClient.cs` returning a signed URL (do not log URL)

- [x] T031 [US2] Implement `GenerateReportAsync` orchestration in `BuyersGuide.Api/Services/Implementations/BuyersGuideService.cs` (validate template belongs to dealer; call `IReportApiClient`; build `ReportResponse`)

- [x] T032 [US2] Implement controller action in `BuyersGuide.Api/Controllers/BuyersGuideController.cs` for `POST /api/BuyersGuide/report`
- [x] T033 [US2] Map validation errors to Problem Details in `BuyersGuide.Api/Controllers/BuyersGuideController.cs` (use 422 for semantic validation issues per contract)
- [x] T034 [US2] Ensure Swagger documents request/response examples in `BuyersGuide.Api/Swagger/SwaggerConfiguration.cs`

### Automated checks (US2)

- [x] T035 [P] [US2] Add integration test for report endpoint happy path in `BuyersGuide.Api.Tests/ReportEndpointTests.cs`
- [x] T036 [P] [US2] Add integration test for invalid VIN validation in `BuyersGuide.Api.Tests/ReportEndpointValidationTests.cs`

**Checkpoint**: US2 passes independent test and tests validate `reportUrl` is present and looks signed.

---

## Phase 5: User Story 3 - Use the report URL safely (Priority: P3)

**Goal**: Ensure `reportUrl` is signed and time-bounded (expiry), and treated as sensitive.

**Independent Test**: Generate a report twice and confirm:

- Each returned `reportUrl` contains an expiry parameter
- Signature changes when expiry changes

### Implementation (US3)

- [x] T037 [US3] Add explicit expiry parameter (e.g., `exp`) to the signed URL format in `BuyersGuide.Api/Helpers/ReportUrlSigner.cs`
- [x] T038 [US3] Add configuration defaulting for expiry + secret in `BuyersGuide.Api/Program.cs` (read from configuration; safe dev defaults)
- [x] T039 [US3] Ensure request logging does not log `reportUrl` in `BuyersGuide.Api/Middleware/RequestLoggingMiddleware.cs`

### Automated checks (US3)

- [x] T040 [P] [US3] Add unit tests for signature determinism and expiry inclusion in `BuyersGuide.Api.Tests/ReportUrlSignerTests.cs`

**Checkpoint**: `reportUrl` includes expiry and signature, and the signer tests are green.

---

## Final Phase: Polish & Cross-Cutting Concerns

- [x] T041 [P] Ensure quickstart commands are accurate and runnable in `specs/001-buyersguide-api/quickstart.md`
- [x] T042 Add a short README for the API project in `BuyersGuide.Api/README.md` (how to run + auth headers + endpoints)
- [x] T043 Confirm Swagger shows examples + required headers for both operations in `BuyersGuide.Api/Swagger/SwaggerConfiguration.cs`

---

## Dependencies & Execution Order

### Story dependency graph

- US1 (P1) → US2 (P2) → US3 (P3)
  - Rationale: US2 reuses template availability checks from US1; US3 extends the URL signing implemented in US2.

### Phase dependencies

- Phase 1 (Setup) blocks Phase 2+
- Phase 2 (Foundational) blocks all user stories
- After Phase 2, US1 can start immediately; US2 can start once the core models and services are in place

---

## Parallel execution examples

### US1 parallel tasks

- [P] Implement `BuyersGuide.Api/Models/BuyersGuide/TemplateDto.cs` (T017)
- [P] Implement `BuyersGuide.Api/Models/BuyersGuide/TemplateResponse.cs` (T018)
- [P] Create `BuyersGuide.Api/Services/Interfaces/IBuyersGuideService.cs` (T019)

### US2 parallel tasks

- [P] Implement `BuyersGuide.Api/Helpers/ReportUrlSigner.cs` (T029)
- [P] Implement `BuyersGuide.Api/Configuration/ReportSigningOptions.cs` (T028)
- [P] Add endpoint tests in `BuyersGuide.Api.Tests/*.cs` (T035, T036)

---

## Implementation strategy

### MVP scope (recommended)

- Implement through US1 only (Phases 1–3) to deliver a usable template listing endpoint with dealer isolation and a contract-valid automated check.

### Incremental delivery

- Add US2 to deliver end-to-end report URL generation.
- Add US3 to harden URL expiry/signature behavior and validate sensitivity constraints.
