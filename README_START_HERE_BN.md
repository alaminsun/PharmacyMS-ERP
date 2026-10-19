# PharmacyMS — All 8 Improvements (Angular 21 + .NET 10)

এই ZIP-এ দুটি project আছে:

- `Backend/PharmacyMS.slnx`: ASP.NET Core 10, EF Core 10, SQL Server LocalDB, JWT, dynamic RBAC
- `AngularClient/`: Angular **21.2.13**, Node 24.11.1 compatible

## শুরু করার আগে — IMPORTANT

**পুরোনো database delete করবে না। `Drop-Database`, `EnsureCreated`, বা migration reset করবে না।**

1. পুরোনো project/database-এর backup নাও। SQL Server LocalDB-এর active development database-এ upgrade করার আগে গুরুত্বপূর্ণ data export করো।
2. Visual Studio 2022 (NET 10 SDK installed) থেকে `Backend/PharmacyMS.slnx` খোলো। `PharmacyMS.API` startup project হবে।
3. আগের project-এর `UserSecretsId` অপরিবর্তিত রাখা হয়েছে। `Jwt:Key` user-secrets-এ থাকলে সেটি ব্যবহার হবে। না থাকলে `SETUP_BN.md` অনুসরণ করো; secrets ZIP-এ নেই।
4. Package Manager Console-এ **Default project** `PharmacyMS.Infrastructure` হওয়া ভালো। Solution root থেকে:

```powershell
Update-Database -Project PharmacyMS.Infrastructure -StartupProject PharmacyMS.API -Context ApplicationDbContext
```

এটি পূর্বের Low Stock migration pending থাকলে সেটি এবং নতুন `AddAuditEvents` migration apply করবে। Migration SQL review করতে চাইলে আগে `Script-Migration` ব্যবহার করো।

5. API Run করে Console-এ listening URL নিশ্চিত করো। সাধারণ development proxy `http://127.0.0.1:5080`-তে API আশা করে। অন্য URL হলে `AngularClient/proxy.conf.json` update করো। Swagger-এ `GET /api/health/ready` check করো।
6. নতুন PowerShell terminal-এ:

```powershell
cd AngularClient
npm install
npm test
npm run build
npm start
```

7. Browser-এ `http://127.0.0.1:4200` খোলো এবং আগের Admin account দিয়ে Login করো।

## আটটি নতুন feature এবং backend mapping

| Feature | Angular | Backend |
|---|---|---|
| 1. Confirm dialog | global ConfirmService, delete/role/status confirmation | existing CRUD |
| 2. Loading skeleton | dashboard, catalog, transactions, stock, reports, admin | existing APIs |
| 3. Server-side advanced table | paged catalog, sales/purchases; sort, search, size | `GET /api/catalog-pages/{key}`, `GET /api/transaction-pages/{kind}` |
| 4. Invoice print / PDF | receipt view, Print → Save as PDF | `GET /api/receipts/{sales|purchases}/{id}` |
| 5. Low/expiry alerts | Stock page 7/30/60 days and dashboard low stock | existing `/api/stock`, `/api/stock/low-stock` |
| 6. FEFO | Sale medicine selection automatically allocates across earliest-expiring nonexpired batches | existing `POST /api/sales` keeps server stock/concurrency checks |
| 7. Audit Logs | `Audit history` admin screen | `GET /api/audit` + AuditEvents migration + middleware |
| 8. Automated E2E tests | Playwright login-to-sale test | live dev/test API |

### Important implementation limitations

- FEFO selection currently occurs in **Angular**; the backend still supports direct batch-specific Sales API requests. It validates stock, expiry and price but does not enforce FEFO for every non-UI client. For mandatory FEFO enforcement across all clients, add a server-side allocation command.
- Audit Trail logs **successful state-changing HTTP API requests** with actor, UTC timestamp, HTTP method, API path, status, trace ID. It deliberately **does not** store passwords, tokens, request/response bodies or row-by-row before/after values. Middleware write is **best effort after the response**, not transactionally atomic with business writes. This is a useful operations history, **not a compliance-grade immutable audit ledger**.
- Invoice print uses browser built-in **Print → Save as PDF**; no server-side PDF generation and no tax authority certification.
- Search for catalog pages matches Name and Code (when available) or Shelf Number; transaction search matches invoice number. Page sizes: 10/20/50; API allows 1–100.
- Alerts use UTC expiry-day comparisons, expired stock excluded from available medicine inventory.

## Playwright E2E

Install dependencies with `npm install`. Then install browser once:

```powershell
npx playwright install chromium
```

In a **new PowerShell terminal**, set temporary test variables and **use test-only accounts**:

```powershell
$env:PHARMACY_E2E_EMAIL = "your-test-user@example.com"
$env:PHARMACY_E2E_PASSWORD = Read-Host "Test account password"
npm run test:e2e
```

The login/navigation test is non-mutating. **Sale test is skipped by default**; to explicitly test a real sale in a disposable TEST database:

```powershell
$env:PHARMACY_MUTATING_E2E = "1"
npm run test:e2e
Remove-Item Env:PHARMACY_MUTATING_E2E
```

Sale test needs at least one medicine with available unexpired stock and user with `Sales.Write` permission. It creates one real Sale; don't enable it on production or shared operational data.

## Suggested manual acceptance checks

1. Admin Login → Dashboard & global toast; reload/refresh skeleton.
2. Category → Add/Edit/Delete → confirmation and success toast.
3. Catalog 25+ records → search, sorting, page size, Next/Previous. Ensure real backend page returns `totalCount`.
4. Purchase 2+ batches for same medicine (different expiry dates) → Sales: select medicine, enter quantity spanning two batches, confirm FEFO preview, Complete sale → stock deduction.
5. Sale/Purchase list → Print / PDF → browser print dialog → Save as PDF; verify historical UnitPrice, Subtotal, Discount, GrandTotal.
6. Stock → 7/30/60 day expiry alerts; Dashboard low/out of stock indicators.
7. Admin → Audit History after a successful Create/Update/Delete (make sure new audit migration was applied).
8. Test Cashier and Pharmacist permission guard: access denied to Admin Audit page unless `Users.Manage` granted.

## Troubleshooting

- `No such table 'AuditEvents'` / SQL table missing → apply migration in the correct database.
- `502/0 Cannot reach API` → check Backend running port and `AngularClient/proxy.conf.json`.
- `403 Forbidden` → permissions are enforced by .NET, UI visibility is not security.
- FEFO insufficient stock → refresh Inventory. Another user may have sold stock since the form opened.
- If build fails, send full `npm run build` and Visual Studio Error List output; local compile/integration could not be run in authoring environment.

## Test transparency

Source/logic contract tests were executed. Actual Angular compiler `ng build`, .NET 10 build, LocalDB migration, live E2E / Playwright browser runs **have not been executed** in this environment because Angular npm packages and .NET SDK are unavailable here. You must perform these checks on Windows.
