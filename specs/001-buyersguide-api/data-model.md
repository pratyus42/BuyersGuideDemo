# Data Model: Buyers Guide Report API

**Feature**: [specs/001-buyersguide-api/spec.md](spec.md)
**Date**: 2026-03-04

This is a conceptual model of the API contracts and internal entities. It is not an implementation.

## Entities

### Dealer

Represents the authorization tenant context.

- `dealerId` (string, required)

Relationships:

- Dealer 1 → N Templates

### Template

A Buyers Guide report template available for a dealer.

- `templateId` (string, required)
- `templateName` (string, required)
- `printableTemplateId` (string, required)
- `dealerId` (string, required)

Validation:

- All ids are non-empty strings.

### ReportRequest (API contract)

Inputs required to generate a report.

- `templateId` (string, required)
- `vin` (string, required)

Validation:

- `templateId`: non-empty
- `vin`: non-empty; basic VIN format validation (17 chars; excludes I/O/Q) as a minimum rule

### TemplateResponse (API contract)

Response payload for listing templates.

- `dealerId` (string, required)
- `templates` (array, required)

`templates[]` fields:

- `templateId`
- `templateName`
- `printableTemplateId`

### ReportResponse (API contract)

Response payload for report generation.

- `vin` (string, required)
- `templateId` (string, required)
- `reportUrl` (string, required; signed and time-bounded)

## State / Transitions

- Template listing is read-only.
- Report generation produces a new `reportUrl` per request.
  - URL includes an expiry timestamp.
  - URL signature is derived from request inputs to prevent tampering.
