# Feature Specification: Buyers Guide Report API

**Feature Branch**: `001-buyersguide-api`  
**Created**: 2026-03-04  
**Status**: Draft  
**Input**: Provide two BuyersGuide endpoints: one to list templates for a dealer and one to generate a Buyers Guide report for a VIN using a chosen template, returning a signed downloadable report URL.

## User Scenarios & Testing *(mandatory)*

<!--
  IMPORTANT: User stories should be PRIORITIZED as user journeys ordered by importance.
  Each user story/journey must be INDEPENDENTLY TESTABLE - meaning if you implement just ONE of them,
  you should still have a viable MVP (Minimum Viable Product) that delivers value.
  
  Assign priorities (P1, P2, P3, etc.) to each story, where P1 is the most critical.
  Think of each story as a standalone slice of functionality that can be:
  - Developed independently
  - Tested independently
  - Deployed independently
  - Demonstrated to users independently
-->

### User Story 1 - List templates for a dealer (Priority: P1)

As an authenticated caller acting for a dealer, I want to retrieve the available Buyers Guide templates for that dealer so I can choose the right format for report generation.

**Why this priority**: Template selection is required before a report can be generated and is the smallest valuable unit that proves dealer-scoped access works.

**Independent Test**: Call the templates endpoint with a valid dealer context and verify a dealer-scoped list of template metadata is returned.

**Acceptance Scenarios**:

1. **Given** an authenticated caller authorized for dealer `D1001`, **When** they request templates for `D1001`, **Then** the system returns a list of templates including `templateId`, `templateName`, and `printableTemplateId`.
2. **Given** an authenticated caller authorized for dealer `D1001`, **When** they request templates for a different dealer, **Then** the system rejects the request (no cross-dealer data exposure).

---

### User Story 2 - Generate a Buyers Guide report (Priority: P2)

As an authenticated caller acting for a dealer, I want to generate a Buyers Guide report for a specific vehicle (identified by VIN) using a selected template so I can provide a downloadable, signed report.

**Why this priority**: This is the primary business outcome: producing a downloadable report based on dealer-selected templates.

**Independent Test**: Submit a request with a known valid template id and a valid VIN and verify a signed report URL is returned.

**Acceptance Scenarios**:

1. **Given** a valid `templateId` and VIN, **When** the caller requests report generation, **Then** the system returns a signed downloadable report URL associated with that template and VIN.
2. **Given** an invalid VIN format, **When** the caller requests report generation, **Then** the system returns a validation error describing the issue.
3. **Given** a `templateId` that is not available for the dealer, **When** the caller requests report generation, **Then** the system rejects the request.

---

### User Story 3 - Use the report URL safely (Priority: P3)

As a caller receiving a report URL, I want the URL to be signed and time-bounded so the report can be downloaded securely without exposing long-lived access.

**Why this priority**: Signed URLs reduce accidental access leakage and are a core security expectation for downloadable artifacts.

**Independent Test**: Generate a report URL and verify it is signed and expires; attempts to use an expired URL fail.

**Acceptance Scenarios**:

1. **Given** a generated report URL, **When** it is used before expiry, **Then** it allows download.
2. **Given** a generated report URL, **When** it is used after expiry, **Then** download is denied.

---

### Edge Cases

- Dealer has zero templates.
- Dealer id is missing/blank/invalid format.
- Caller is unauthenticated or unauthorized for the dealer.
- Template id is missing/blank/unknown.
- VIN is missing/blank/invalid format.
- Report generation fails internally (transient vs. permanent failures).
- The system must not log or expose signed download URLs.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST expose exactly two BuyersGuide API operations: list templates and generate a report.
- **FR-002**: System MUST require authentication for both operations.
- **FR-003**: System MUST enforce dealer isolation: callers can only access templates and reports for their authorized dealer context.

- **FR-004**: Templates operation MUST accept a dealer identifier and return template metadata entries containing `templateId`, `templateName`, and `printableTemplateId`.
- **FR-005**: Templates operation MUST return an empty list (not an error) when a dealer has no templates.

- **FR-006**: Report generation MUST accept a template identifier and vehicle identifier (VIN) at minimum.
- **FR-007**: Report generation MUST validate inputs and return a clear validation error for missing/invalid `templateId` or VIN.
- **FR-008**: Report generation MUST only use templates that are available to the dealer context.
- **FR-009**: Report generation MUST return a signed, time-bounded downloadable URL.
- **FR-010**: The `reportUrl` MUST allow the caller to download a report file that corresponds to the requested VIN and template.

- **FR-011**: The API MUST return a consistent, documented error response shape across endpoints.
- **FR-012**: The API MUST support a correlation/request id so requests can be traced end-to-end.
- **FR-013**: The API MUST produce operational logs for successes and failures, while not logging sensitive values (including signed URLs).
- **FR-014**: The API MUST provide a human-friendly, interactive API documentation page that includes example requests and responses for both endpoints.

### Assumptions

- Dealers and templates are sourced from a controlled dataset for initial delivery (e.g., mock mappings) and can be replaced later without changing the API contract.
- The initial dataset includes at least dealers `D1001` (with two templates) and `D2001` (with one template) to enable repeatable testing.
- Signed report URLs expire after a short period (assume 15 minutes) unless overridden by policy.

### Dependencies

- A source of truth for the authenticated caller’s dealer authorization context.
- A template catalog scoped to dealer.
- A mechanism to generate a signed, time-bounded downloadable link for the produced report.

### Key Entities *(include if feature involves data)*

- **Dealer**: The tenant context used for authorization and data isolation.
- **Template**: A report layout option available to a dealer.
  - Attributes: `templateId`, `templateName`, `printableTemplateId`
- **Report Request**: Inputs required to generate a report.
  - Attributes: `templateId`, `vin` (and optionally other report parameters)
- **Report Response**: Output that allows the caller to download the generated report.
  - Attributes: `reportUrl` (signed, time-bounded), `templateId`, `vin`

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: For valid, authorized requests, 95% of template list requests return a response in under 1 second.
- **SC-002**: For valid, authorized requests, 95% of report generation requests return a response (including a signed download URL) in under 5 seconds.
- **SC-003**: 100% of responses for templates include the required fields (`templateId`, `templateName`, `printableTemplateId`) when templates are returned.
- **SC-004**: 0 confirmed incidents of cross-dealer data exposure from these endpoints during testing.
- **SC-005**: Signed report URLs expire within the configured expiry window (default assumption: 15 minutes).
- **SC-006**: An interactive API documentation page is accessible and shows example requests/responses for both endpoints.
