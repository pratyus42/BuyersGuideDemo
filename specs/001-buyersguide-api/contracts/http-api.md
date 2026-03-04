# HTTP API Contract: Buyers Guide Report API

**Base route**: `/api/BuyersGuide`

## Cross-cutting

### Authentication (mock mode)

Requests MUST include:

- `X-Api-Key: <string>`
- `X-Dealer-Id: <dealerId>` (authenticated dealer context)

If authentication fails, the API returns `401` with Problem Details.

### Correlation ID

- Request may include `X-Correlation-Id`.
- Response MUST include `X-Correlation-Id`.

### Error format

All non-2xx responses MUST be returned as Problem Details JSON (RFC 7807), with:

- `type` (string)
- `title` (string)
- `status` (number)
- `detail` (string)
- `instance` (string)

The API MUST NOT include secrets, signed URLs, or stack traces in error responses.

---

## 1) List dealer templates

### Request

`GET /api/BuyersGuide/templates?dealerId={dealerId}`

Query parameters:

- `dealerId` (string, required)

Authorization rules:

- `dealerId` MUST equal the authenticated dealer context from `X-Dealer-Id`.
  - If not, return `403`.

### Response (200)

```json
{
  "dealerId": "D1001",
  "templates": [
    {
      "templateId": "TMP001",
      "templateName": "Standard Buyers Guide",
      "printableTemplateId": "PRT001"
    },
    {
      "templateId": "TMP002",
      "templateName": "Warranty Buyers Guide",
      "printableTemplateId": "PRT002"
    }
  ]
}
```

### Errors

- `400`: missing/invalid `dealerId`
- `401`: unauthenticated
- `403`: dealer mismatch / unauthorized
- `500`: unexpected error

---

## 2) Generate Buyers Guide report

### Request

`POST /api/BuyersGuide/report`

Body:

```json
{
  "templateId": "TMP001",
  "vin": "1HGCM82633A123456"
}
```

Authorization rules:

- `templateId` MUST be available to the authenticated dealer (`X-Dealer-Id`).
  - If not, return `403` (or `404` if policy prefers not to reveal existence).

### Response (200)

```json
{
  "vin": "1HGCM82633A123456",
  "templateId": "TMP001",
  "reportUrl": "https://mockreports.service.com/buyersguide/report123?exp=1760000000&signature=xyz"
}
```

### Errors

- `400`: invalid JSON body
- `401`: unauthenticated
- `403`: template not accessible for dealer
- `422`: validation error (missing/invalid `templateId` or `vin`)
- `500`: unexpected error
