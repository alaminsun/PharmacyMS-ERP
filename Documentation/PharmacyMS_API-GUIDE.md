# PharmacyMS ERP v1.0.0 — REST API Guide

**Audience:** developers and technical buyers  
**Stack:** ASP.NET Core 10, EF Core, SQL Server, ASP.NET Core Identity, JWT, MediatR  
**API base path:** `/api`  
**Status:** Buyer-facing reference based on the controller/DTO source present on GitHub `main` as inspected on 21 October 2026. Verify against the final release source and running OpenAPI contract before publishing.

## 1. Getting started

Set `BASE_URL` to **your own deployment**, not the seller's hosted demo. For local development, an example is `http://127.0.0.1:5080`. Most endpoints require a JWT access token. Send it as:

```http
Authorization: Bearer <YOUR_ACCESS_TOKEN>
Content-Type: application/json
```

Do not hardcode tokens in source code, screenshots, documentation, or commits. Always use HTTPS for remote deployments. The `/api/health` and `/api/health/ready` endpoints are anonymous; readiness may reveal database availability and should be protected appropriately by the deployment perimeter if necessary.

Development-only Swagger UI is available at `/swagger` when the API runs in the `Development` environment. It is not enabled by default in `Production`.

## 2. Authentication

| Method | Endpoint | Authorization | Purpose |
|---|---|---|---|
| POST | `/api/auth/login` | Anonymous | Sign in and receive a JWT |
| GET | `/api/auth/me` | Authenticated | Current user ID and email |
| GET | `/api/auth/me/permissions` | Authenticated | Effective roles and permissions |

**POST `/api/auth/login`**

```json
{
  "email": "test.operator@example.test",
  "password": "<PASSWORD_ENTERED_SECURELY>"
}
```

A successful response contains `accessToken`, `expiresAtUtc`, `email`, and `roles`. Invalid credentials return HTTP `401`. Account status, lockout, security stamp, expiry, and permissions may also affect authorized requests.

### PowerShell example (do not print your token)

```powershell
$baseUrl = 'http://127.0.0.1:5080'
$email = 'test.operator@example.test'
# Supply your password using a secure local method; never paste secrets in tickets.
$payload = @{ email = $email; password = '<ENTER_PASSWORD_LOCALLY>' } | ConvertTo-Json
$login = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -ContentType 'application/json' -Body $payload
$headers = @{ Authorization = "Bearer $($login.accessToken)" }
Invoke-RestMethod -Uri "$baseUrl/api/auth/me/permissions" -Headers $headers
```

The snippet illustrates the API shape; replace the placeholder locally and avoid leaving passwords in PowerShell history. Do not use real credentials in a public demonstration.

## 3. API access and permissions

| Resource | Read policy | Write policy |
|---|---|---|
| Medicines | `Medicines.Read` | `Medicines.Write` |
| Categories | `Categories.Read` | `Categories.Write` |
| Generics | `Generics.Read` | `Generics.Write` |
| Shelves | `Shelves.Read` | `Shelves.Write` |
| Suppliers | `Suppliers.Read` | `Suppliers.Write` |
| Customers | `Customers.Read` | `Customers.Write` |
| Purchases | `Purchases.Read` | `Purchases.Write` |
| Sales | `Sales.Read` | `Sales.Write` |
| Stock / low stock | `Stock.Read` | Not applicable |
| Dashboard | `Dashboard.Read` | Not applicable |
| Reports | `Reports.Read` | Not applicable |
| User management / audit | `Users.Manage` | `Users.Manage` where applicable |
| Role management | `Roles.Manage` | `Roles.Manage` where applicable |

The frontend's hidden menus are not security boundaries; controllers enforce access independently. Depending on the request, HTTP `401` means authentication is missing/invalid; HTTP `403` means the authenticated user lacks permission. The `Admin` role may be treated specially by the permission handler, so verify effective behavior in the final release.

## 4. Catalog and medicines

The following catalog resources support the same basic REST operations:

| Resource path | GET collection | GET `/{id}` | POST | PUT `/{id}` | DELETE `/{id}` |
|---|---|---|---|---|---|
| `/api/medicines` | Yes | Yes | Yes | Yes | Yes* |
| `/api/categories` | Yes | Yes | Yes | Yes | Yes |
| `/api/generics` | Yes | Yes | Yes | Yes | Yes |
| `/api/shelves` | Yes | Yes | Yes | Yes | Yes |
| `/api/suppliers` | Yes | Yes | Yes | Yes | Yes |
| `/api/customers` | Yes | Yes | Yes | Yes | Yes |

\*Medicine DELETE deactivates a record rather than deleting transaction history. Some referenced records cannot be deleted and may return a conflict. Read and write operations require their corresponding resource policies.

**Create a medicine — POST `/api/medicines`**

```json
{
  "code": "DEMO-MED-001",
  "name": "Sample Product",
  "strength": "100 mg",
  "categoryId": null,
  "genericId": null,
  "shelfId": null,
  "supplierId": null,
  "buyingPrice": 10.00,
  "sellingPrice": 15.00,
  "isActive": true,
  "minimumStockLevel": 10
}
```

The request uses the `MedicineInput` DTO. The creation endpoint returns HTTP `201 Created` with an ID; update and deactivate return HTTP `204 No Content` when successful. The medicine collection supports the optional `search` query parameter, for example `GET /api/medicines?search=DEMO`.

## 5. Purchase workflow

| Method | Endpoint | Permission |
|---|---|---|
| GET | `/api/purchases` | `Purchases.Read` |
| POST | `/api/purchases` | `Purchases.Write` |

**POST `/api/purchases`** — test example (creates a real transaction; use a dedicated test database):

```json
{
  "invoiceNumber": "DEMO-PUR-001",
  "supplierId": 1,
  "notes": "Sample purchase for testing",
  "discount": 0,
  "items": [
    {
      "medicineId": 1,
      "batchNumber": "DEMO-BATCH-001",
      "expiryDate": "2028-12-31",
      "quantity": 10,
      "unitPrice": 8.50
    }
  ]
}
```

`SupplierId` and `MedicineId` must refer to existing records. The API creates or updates batch stock according to its transaction logic. A successful response is HTTP `200 OK`, with an `id` property. Purchases are not exposed as editable historical transactions through the controller shown in this version.

## 6. Sales workflow

| Method | Endpoint | Permission |
|---|---|---|
| GET | `/api/sales` | `Sales.Read` |
| POST | `/api/sales` | `Sales.Write` |

Before creating a sale, query `/api/stock` to obtain an actual `stockBatchId` and verify remaining quantity and expiry date.

**POST `/api/sales`** — test example (will deduct inventory if accepted):

```json
{
  "invoiceNumber": "DEMO-SAL-001",
  "customerId": null,
  "notes": "Sample walk-in sale",
  "discount": 0,
  "items": [
    { "stockBatchId": 1, "quantity": 2 }
  ]
}
```

A successful response is HTTP `200 OK`, with an `id`. Selling price comes from the server-side medicine record, not from a client-supplied `unitPrice`. The backend checks quantities and expiry. **The selected batch ID is provided by the client**; do not claim the backend always chooses the earliest-expiring batch unless that is implemented and tested in the final release.

## 7. Stock, dashboard, reports and receipts

| Method | Endpoint | Permission / Notes |
|---|---|---|
| GET | `/api/stock?includeZero=false` | `Stock.Read` |
| GET | `/api/stock/low-stock` | `Stock.Read` |
| GET | `/api/dashboard` | `Dashboard.Read` |
| GET | `/api/reports?fromUtc=2026-01-01T00:00:00Z&toUtc=2027-01-01T00:00:00Z` | `Reports.Read` |
| GET | `/api/reports/sales-details?fromUtc=...&toUtc=...` | `Reports.Read` |
| GET | `/api/receipts/sales/{id}` | `Sales.Read` |
| GET | `/api/receipts/purchases/{id}` | `Purchases.Read` |

Stock records contain `stockBatchId`, `medicineId`, `medicineCode`, `medicineName`, `batchNumber`, `expiryDate`, `quantityOnHand`, and `isExpired`. Receipt responses include invoice details, party, subtotal, discount, total, and item lines; printing or saving to PDF is a client-side workflow, not a PDF-download API endpoint in the inspected controller.

## 8. Pagination and search

### Catalog

`GET /api/catalog-pages/{key}?search=sample&sortBy=Name&descending=false&page=1&pageSize=20`

Allowed `{key}` values are `medicines`, `categories`, `generics`, `shelves`, `suppliers`, and `customers`. The relevant `.Read` permission is required. `page` must be at least 1 and `pageSize` must be 1–100. Supported sort properties depend on the entity.

### Transactions

`GET /api/transaction-pages/{kind}?search=DEMO&page=1&pageSize=20`

`{kind}` is `sales` or `purchases`. Search filters invoice number. Valid sort keys include `grandtotal` and `invoicenumber`; otherwise a transaction date order is used. The corresponding read permission is required.

### Common page response shape

```json
{
  "items": [],
  "totalCount": 0,
  "page": 1,
  "pageSize": 20
}
```

## 9. User administration

The following endpoints require `Users.Manage` (typically available to an administrator):

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/admin/users` | List users |
| POST | `/api/admin/users` | Create user |
| POST | `/api/admin/users/assign-role` | Assign role |
| POST | `/api/admin/users/remove-role` | Remove role |
| PUT | `/api/admin/users/status` | Activate or deactivate user |

**Create user (illustrative):**

```json
{
  "email": "new.cashier@example.test",
  "password": "<ENTER_SECURE_PASSWORD>",
  "role": "Cashier"
}
```

**Change status:**

```json
{ "email": "new.cashier@example.test", "isActive": false }
```

Store credentials securely. Administrative endpoints must not be exposed as anonymous endpoints. Create user returns HTTP `201`; role changes and status updates return `204` on success.

## 10. Role and permission administration

These endpoints require `Roles.Manage`:

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/admin/roles` | List roles |
| GET | `/api/admin/roles/permissions` | List defined permission names |
| POST | `/api/admin/roles` | Create role |
| POST | `/api/admin/roles/grant-permission` | Grant permission |
| POST | `/api/admin/roles/revoke-permission` | Revoke permission |
| POST | `/api/admin/roles/seed-defaults` | Initialize missing default roles |

Permission request example:

```json
{ "role": "Cashier", "permission": "Sales.Read" }
```

Do not assume that running default-role seeding resets previously revoked permissions; inspect the actual role seeding implementation in the installed release.

## 11. Audit history

`GET /api/audit?page=1&pageSize=25&actorEmail=&resource=`

Requires `Users.Manage`. `pageSize` must be 1–100. Optional `actorEmail` and `resource` filters match actor email and API path, respectively. Results are ordered newest-first and contain UTC timestamp, actor, action, path, HTTP status, and trace ID. These are API metadata audit records, **not** a substitute for an immutable accounting ledger or complete regulatory compliance logging.

## 12. Health checks

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/health` | Basic service response |
| GET | `/api/health/ready` | SQL Server connectivity check; may return HTTP `503` |

Do not interpret a successful health check as proof that all permissions, business functions, integrations, and backups are valid.

## 13. HTTP status codes

| Code | Meaning |
|---|---|
| `200` | Successful read, command, or response with body |
| `201` | Resource created |
| `204` | Successful request without response body |
| `400` | Invalid input, paging parameters, or validation failure (depends on handler) |
| `401` | Invalid, absent, expired, or revoked authentication |
| `403` | Authenticated caller lacks authorization |
| `404` | Requested route or record not found |
| `409` | Possible domain conflict, e.g. deleting an in-use reference (depending on exception mapping) |
| `500` | Server error; inspect server logs without exposing secrets |
| `503` | Readiness database connection unavailable |

## 14. Integration checklist for a new buyer

1. Apply the **included** EF Core migrations to an empty, buyer-controlled SQL Server database.
2. Configure unique credentials and JWT signing key through secrets/environment variables; never ship shared defaults.
3. Provision an administrator using the final release's one-time CLI procedure, then clear provisioning variables.
4. Sign in; call `/api/auth/me/permissions`.
5. Create test category, supplier, medicine, purchase batch, and sale on a **non-production** database.
6. Verify stock quantities, receipts, reports, and role denials (`403`).
7. Confirm HTTPS, backups, health checks, rate limits, and CORS for the buyer's domain.

## 15. Important release caveats

- This reference reflects inspected GitHub `main` controller and DTO code; your local working tree contains newer Admin provisioning changes that had not yet been pushed when this guide was prepared. Update the release source and re-check the guide together.
- The buyer's backend URL, database host, frontend domain, JWT key, and credentials must all be replaced. The seller's Railway and Vercel domains are **demo-only** and should not be used as buyer production dependencies.
- Purchases and sales have real database side effects. Use isolated test data during integration testing.
- This is an API reference for the currently implemented product, not a claim of pharmacy licensing, local tax compliance, full accounting capabilities, or medical regulatory certification.

**Technical references:** `Backend/PharmacyMS.API/Controllers/*.cs` and `Backend/PharmacyMS.Application/DTOs/*.cs`, repository `alaminsun/PharmacyMS-ERP`, `main` branch (inspected 21 October 2026).
