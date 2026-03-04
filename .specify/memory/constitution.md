<!--
Sync Impact Report

- Version change: template placeholder → 1.0.0
- Modified principles:
	- Principle 1 placeholder → I. Contract-First API (Source of Truth)
	- Principle 2 placeholder → II. Security & Dealer Isolation
	- Principle 3 placeholder → III. Inputs, Outputs, and Errors Are Explicit
	- Principle 4 placeholder → IV. Signed Report URLs Are Treated as Secrets
	- Principle 5 placeholder → V. Operability Is Built-In
- Added sections:
	- API Contract (Non-Negotiable)
	- Development Workflow & Quality Gates
- Removed sections: none
- Templates requiring updates:
	- ✅ .specify/templates/plan-template.md
	- ✅ .specify/templates/tasks-template.md
	- ⚠ .specify/templates/spec-template.md (no change needed for alignment)
	- ⚠ .specify/templates/checklist-template.md (no change needed for alignment)
- Follow-up TODOs: none
-->

# ExternalAPI BuyersGuide API Constitution

## Core Principles

### I. Contract-First API (Source of Truth)
The API contract MUST be the source of truth for behavior and compatibility.

- The only endpoints in scope are:
	- `GET /api/BuyersGuide/templates`
	- `POST /api/BuyersGuide/report`
- Any change to request/response fields, status codes, or semantics MUST update the
	contract in the same PR.
- Breaking changes MUST follow the Versioning policy in Governance.

### II. Security & Dealer Isolation
Requests MUST be authenticated and MUST be authorized to the dealer context.

- The caller identity MUST be established via the service’s standard auth mechanism
	(implementation-specific, but mandatory).
- If `dealerId` is provided as an input, it MUST match the authenticated dealer context;
	otherwise the request MUST be rejected.
- No cross-dealer data leakage: templates and reports returned MUST belong to exactly
	one dealer.

### III. Inputs, Outputs, and Errors Are Explicit
Every endpoint MUST have explicit inputs, deterministic outputs, and a consistent error
shape.

- Validate required parameters (e.g., `dealerId`, `templateId`, `vin`) and reject invalid
	inputs with clear, consistent error responses.
- Use appropriate HTTP status codes (e.g., 200/201, 400, 401, 403, 404, 409, 422, 500).
- Error responses MUST be safe (no stack traces, secrets, or internal URLs).

### IV. Signed Report URLs Are Treated as Secrets
Signed report URLs MUST be treated as sensitive and handled accordingly.

- The report endpoint MUST return a downloadable URL that is signed and time-bounded.
- Signed URLs MUST NOT be logged.
- The report MUST be generated from the selected template and requested parameters
	(e.g., VIN) without silently substituting other templates.

### V. Operability Is Built-In
The API MUST be diagnosable in production without exposing sensitive data.

- Structured logs MUST include: endpoint name, correlation/request id, authenticated
	dealer identity (or dealer hash), and high-level outcome (success/failure + status).
- Logs MUST NOT include: VIN (unless policy explicitly allows), signed URLs, or secrets.
- All failures MUST be mapped to the standard error shape.

## API Contract (Non-Negotiable)

This constitution does not replace a full contract file, but it defines the minimum
behavior that MUST remain true.

### Endpoint: `GET /api/BuyersGuide/templates`

Purpose: For a given dealer context, return template metadata used to generate reports.

Minimum response fields (per template):

- `templateId`
- `templateName`
- `printableTemplateId`

### Endpoint: `POST /api/BuyersGuide/report`

Purpose: Generate a BuyersGuide report from a template and VIN and return a downloadable
signed report URL.

Minimum inputs:

- `templateId`
- `vin`

Minimum response fields:

- `reportUrl` (signed, time-bounded)

If report generation is asynchronous in the implementation, the contract MUST still be
explicit about the client flow (e.g., polling status vs. immediate URL).

## Development Workflow & Quality Gates

- Any PR that changes the API surface MUST update the contract and include at least
	one automated check proving the change (contract test, integration test, or equivalent).
- Changes that affect signing, authorization, or URL generation MUST include a security
	review in PR notes.
- Every release MUST be tagged with the API version from Governance.

## Governance
<!-- Example: Constitution supersedes all other practices; Amendments require documentation, approval, migration plan -->

This constitution supersedes all other development conventions in this repository.

- **Amendments**: Any change MUST be made via PR with a clear rationale, impact summary,
	and (if contract-related) a migration note.
- **Compliance**: Reviews MUST explicitly verify compliance with the Core Principles.
- **Versioning**: Semantic versioning (`MAJOR.MINOR.PATCH`).
	- MAJOR: breaking contract changes (removing fields, changing meanings, incompatible
		auth/authorization semantics)
	- MINOR: backwards-compatible additions (new optional fields, new error codes)
	- PATCH: clarifications, bug fixes that do not change the contract

**Version**: 1.0.0 | **Ratified**: 2026-03-03 | **Last Amended**: 2026-03-03
<!-- Example: Version: 2.1.1 | Ratified: 2025-06-13 | Last Amended: 2025-07-16 -->
