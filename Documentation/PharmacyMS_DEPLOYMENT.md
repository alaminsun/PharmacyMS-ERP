# PharmacyMS ERP v1.0.0 — Deployment Guide

**Audience:** Buyers and developers deploying their own licensed copy of PharmacyMS.

**Stack:** ASP.NET Core 10 / .NET 10, Entity Framework Core, SQL Server, Angular 21, Node.js, Railway (API), and Vercel (frontend).

> **Release compatibility notice:** These instructions assume the buyer's source package includes the updated `--provision-admin` command and the restricted development-only startup bootstrap. The GitHub `main` branch inspected during preparation still contained the older opt-in production bootstrap. Confirm your release ZIP contains the newer, tested local changes before distributing it.
>
> Hosting providers regularly change their dashboards, pricing, limits, and build settings. Check current platform documentation and your plan before deploying. A production deployment needs a hosting plan that permits its intended commercial use.

## 1. Architecture and deployment boundaries

```text
Browser
  |
  | HTTPS
  v
Vercel — Angular static application
  |
  | /api/* rewrite (same-origin browser requests)
  v
Railway — ASP.NET Core API (Docker, port 8080)
  |
  | Encrypted database connection
  v
Buyer-owned SQL Server database
```

Both hosting services and the SQL Server must be controlled by the buyer. **Do not use the seller's demo database or production credentials.** The example public demo URLs in the original project are for demonstration only.

Project layout (from the repository root):

- `Backend/PharmacyMS.API` — application entry point and API.
- `Backend/PharmacyMS.Infrastructure` — EF Core DbContext and migrations.
- `Backend/Dockerfile` — multi-stage .NET 10 image; listens on port `8080`.
- `Backend/PharmacyMS.slnx` — .NET solution.
- `AngularClient` — Angular application, `package.json` and `vercel.json`.

## 2. Prerequisites

- Git or an extracted source ZIP.
- .NET 10 SDK and matching `dotnet-ef` 10.x for applying migrations.
- Node.js compatible with `AngularClient/package.json` (currently Node `^20.19.0 || ^22.12.0 || ^24.0.0`) and npm.
- Buyer-owned SQL Server instance reachable from the API host; create a dedicated database and application login with the minimum necessary permissions.
- Railway project/service access, Vercel project access and the required paid/commercial-use entitlements.
- A secure method for storing secrets and a plan for database backups and restores.

Windows LocalDB is appropriate for local development only. **It is not a SQL Server host for Railway.**

## 3. Prepare the source and run release checks

Open PowerShell in the extracted project root:

```powershell
cd '<YOUR_PROJECT_FOLDER>'
dotnet build .\Backend\PharmacyMS.slnx -c Release
dotnet test .\Backend\PharmacyMS.slnx -c Release
cd .\AngularClient
npm ci
npm run build
```

Confirm Angular produced `dist/pharmacyms-angular-client/browser/index.html`. The current project has four verified xUnit domain tests; passing them is **not** a comprehensive security or end-to-end certification.

## 4. Prepare the SQL Server database

1. Provision a dedicated SQL Server database on a trusted host. Ensure it accepts connections from Railway via an appropriately restricted network path and TLS.
2. Create a distinct application database login. **Do not use the SQL Server `sa` account** for ongoing application access.
3. Set aside a separate, time-limited migration identity if the normal application login must not have schema-change privileges.
4. Record the connection string in a password manager/secret store; never commit it to source control or screenshots.
5. Test database connectivity from the expected deployment network.

**Migration safety:** Use the EF Core migrations already included in `Backend/PharmacyMS.Infrastructure/Migrations`. Do **not** run `dotnet ef migrations add InitialCreate` on a buyer installation with those migrations already present. Do not point a fresh-install command at any existing business database without an inspected backup and a migration plan.

### Apply migrations from a trusted workstation or CI job

From the `Backend` directory, set the connection string in the **current shell only** using a secure input or approved secret manager. The following is the EF command once `ConnectionStrings__PharmacyConnection` is set:

```powershell
cd .\Backend

dotnet ef database update `
  --project .\PharmacyMS.Infrastructure\PharmacyMS.Infrastructure.csproj `
  --startup-project .\PharmacyMS.API\PharmacyMS.API.csproj `
  --context ApplicationDbContext `
  --connection $env:ConnectionStrings__PharmacyConnection
```

If the EF tooling needs `Jwt:Key` during startup, configure a distinct, randomly generated temporary key in `Jwt__Key` for this tooling session; do not copy the seller's production key. If using an environment variable for the DB connection string, clear it from the session afterwards. Verify the migration is applied to the **intended** database by inspecting the target and its `__EFMigrationsHistory` table.

> The project deliberately does not apply schema migrations automatically when the API starts. Migrations are a separate deployment operation.

## 5. Configure secrets and runtime settings

Set the following on the **Railway API service** under its deployment environment variables. Values shown here are **names or descriptions**, not credentials:

| Variable | Purpose |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__PharmacyConnection` | Buyer-owned production SQL Server connection string |
| `Jwt__Key` | Strong unique random JWT signing key, at least 32 UTF-8 bytes; keep secret |
| `Jwt__Issuer` | JWT issuer, such as `PharmacyMS.API`, consistently configured |
| `Jwt__Audience` | JWT audience, such as `PharmacyMS.Client`, consistently configured |
| `ASPNETCORE_URLS` | `http://0.0.0.0:8080` (already defined in Dockerfile) |
| `AllowedHosts` | Actual API hostname, or properly configured host list |
| `AllowedOrigins__0` | Optional allowed browser origin; use only the actual frontend domain when cross-origin requests are necessary |

`__` maps to `:` in ASP.NET Core configuration. Do not supply JWT secrets to the Angular app or Vercel public environment variables. Avoid wildcard CORS origins. When the frontend uses the same-origin `/api` rewrite, it can usually call the API without browser cross-origin access.

**Production bootstrap variables:** Remove legacy `Bootstrap__RunOnce`, `Bootstrap__Email`, and `Bootstrap__Password` from the long-running Railway service after initial provisioning. The current release should not have an automatic production startup bootstrap.

## 6. Deploy the ASP.NET Core API to Railway

1. Import or connect your private repository in Railway and create an API service.
2. Set the service **Root Directory** to `/Backend` so the Docker build sees `PharmacyMS.API` and the other project directories.
3. Build with `Backend/Dockerfile` (relative to the repository root), or configure Railway's Dockerfile path according to the current project-root settings. The file uses a .NET 10 SDK build stage and .NET 10 ASP.NET runtime stage.
4. Configure the environment variables from section 5 as **secrets**.
5. Expose the application's HTTP port **8080** under Railway networking settings and enable HTTPS on the public Railway domain/custom domain.
6. Deploy; inspect build logs for successful `dotnet publish` and application startup. Do not log credentials or secret values.
7. Test API health from a browser or terminal:

```powershell
Invoke-RestMethod 'https://YOUR-API-DOMAIN/api/health'
Invoke-RestMethod 'https://YOUR-API-DOMAIN/api/health/ready'
```

`/api/health` shows process-level health; `/api/health/ready` tests database connectivity. Both are currently anonymous endpoints. Avoid exposing unnecessary diagnostic information; consider network controls for readiness probes in sensitive installations.

If `/api/health` works but `/api/health/ready` returns HTTP 503, check SQL connectivity, firewall rules, database credentials, TLS requirements, and the expected database name.

## 7. Provision the first Admin user (one-time task)

The updated application supports a CLI-only `--provision-admin` mode, tested against a separate LocalDB test database. **Never expose Admin provisioning over a public HTTP endpoint.**

For a buyer-owned deployment, run the *built API* as a one-off process in a trusted environment that has access to the target database and the same essential application configuration. Provide `Bootstrap__Email` and `Bootstrap__Password` temporarily through a secure secret-injection mechanism; also provide `Jwt__Key` because the current API validates the JWT key before the one-time command branch.

From a trusted machine with the .NET SDK and an explicitly verified database configuration:

```powershell
cd .\Backend
# Set these environment variables securely for this one process/session:
#   ASPNETCORE_ENVIRONMENT=Production
#   ConnectionStrings__PharmacyConnection=<BUYER_DB_CONNECTION>
#   Jwt__Key=<NEW_RANDOM_SECRET>
#   Bootstrap__Email=<BUYER_ADMIN_EMAIL>
#   Bootstrap__Password=<BUYER_ADMIN_PASSWORD>

dotnet run `
  --project .\PharmacyMS.API\PharmacyMS.API.csproj `
  -c Release `
  --no-launch-profile `
  -- --provision-admin
```

**Do not literally paste the bracketed placeholders into a working secret setting.** Avoid command-line passwords, checked-in files, shared shell history, and public logs. Verify that the connection is pointing to the buyer database before launching. Clear temporary environment variables immediately after provisioning. A second invocation with the same existing Admin email should not create another account; this was tested on a local isolated database.

**Railway note:** A normal web-service deployment is **not** a one-off admin-provisioning job. Do not replace the production service start command with `--provision-admin` and leave it there. Use a controlled one-off job/container, private runner, or trusted workstation with database connectivity and inject secrets only for that execution. Deployment-specific job mechanisms need verification with the buyer's Railway account.

## 8. Configure and deploy Angular to Vercel

**Important: change the hardcoded API destination in the supplied frontend.** The repository's current `AngularClient/vercel.json` routes `/api/:path*` to the seller's demonstration API domain. A buyer **must** replace that destination with their own Railway API hostname *before deployment*.

Example `AngularClient/vercel.json` (replace `YOUR-API-DOMAIN`):

```json
{
  "$schema": "https://openapi.vercel.sh/vercel.json",
  "rewrites": [
    {
      "source": "/api/:path*",
      "destination": "https://YOUR-API-DOMAIN/api/:path*"
    },
    {
      "source": "/(.*)",
      "destination": "/index.html"
    }
  ]
}
```

The order matters: API rewrite first, single-page-application fallback second. Verify the rewritten destination is exactly your controlled API and uses HTTPS.

Create a Vercel project from the same repo:

| Setting | Value |
| --- | --- |
| Root Directory | `AngularClient` |
| Install Command | `npm ci` |
| Build Command | `npm run build` |
| Output Directory | `dist/pharmacyms-angular-client/browser` |
| Framework | Angular, if correctly detected; verify output settings |

After deployment, check that refreshing a nested Angular route loads the SPA and browser network requests go to `/api/...` on the Vercel origin, which Vercel forwards to the buyer's API. Verify there are no requests to the seller's example hostname.

Vercel plan terms and usage limits change. Ensure the chosen plan permits the buyer's commercial application and resource usage.

## 9. Production verification and security checklist

Do not announce the installation as production-ready merely because the build succeeds. Verify these steps against the **buyer database**, with synthetic test data and without exposing admin secrets:

- [ ] Migrations applied to the intended SQL Server database.
- [ ] Buyer Admin can sign in; no seller/demo credentials remain.
- [ ] Buyer Admin can create a restricted Cashier user and view Roles & Permissions.
- [ ] Cashier can access permitted Sales, Customer, Medicine-read, and Stock-read features.
- [ ] Cashier receives `403 Forbidden` when attempting forbidden Purchases or Admin APIs with a valid token.
- [ ] Purchase adds batch stock; Sale reduces the expected batch; totals and reports reconcile.
- [ ] Expired and insufficient-stock sale attempts are rejected.
- [ ] `/api/health` and `/api/health/ready` behave as expected.
- [ ] Audit History contains the relevant API metadata events without sensitive request bodies.
- [ ] All public endpoints use HTTPS; database communications use suitable encryption and firewall restrictions.
- [ ] Backups are scheduled and a restore has been tested.
- [ ] Secret scans and dependency vulnerability checks have been reviewed.
- [ ] Frontend has no hardcoded seller API URL; runtime secrets exist only in server-side secret storage.

**Product scope caution:** This application is a pharmacy inventory, purchase and sales management system. Do not claim full accounting, legally certified medical/pharmacy compliance, guaranteed tax-compliant receipts, immutable audit accounting, or server-enforced FEFO beyond what the delivered code and independent tests actually demonstrate. Comply with regulations in the buyer's jurisdiction before using real patient or customer records.

## 10. Troubleshooting

| Symptom | Likely cause / action |
| --- | --- |
| Startup fails: `Configure Jwt:Key` | Configure a strong `Jwt__Key` in Railway service secrets; ensure the key is at least 32 UTF-8 bytes. |
| Health works; readiness returns 503 | Test database host, port, connectivity, credentials, TLS, and correct target database. |
| Login works locally but not on Vercel | Inspect `/api` rewrite, deployed API domain and browser DevTools network response. |
| Angular route returns 404 on refresh | Confirm Vercel SPA rewrite exists after the API rewrite. |
| Cashier sees `401` | Token expired/invalid or account inactive; log in again and inspect authentication. |
| Cashier sees `403` | Token authenticated but permission denied. Check assigned role/permission and intended route. |
| Admin CLI exits before provisioning | Verify required `Jwt__Key`, database connection, and temporary bootstrap email/password are present in the one-off environment. |
| EF migrations target the wrong database | Stop; inspect `--connection` and environment configuration before rerunning. |

## 11. Backup, upgrades and operations

- Create a database backup **before** any schema upgrade; test restore periodically.
- Apply updates to a staging environment first, then roll out a tested version to production.
- Coordinate API and frontend releases; roll back compatible binaries/configuration if a rollout fails.
- Rotate credentials if accidentally exposed; changing a committed file does not erase old Git history.
- Review API logs, audit logs, failed sign-ins, DB capacity, and provider usage regularly.
- Never include live data, backup files, `.env` secrets, `.vs`, `node_modules`, `bin`, `obj`, or build output in the distributed source ZIP unless explicitly justified and sanitized.

## 12. Release-owner verification notes

This document was prepared from the repository files `Backend/Dockerfile`, `AngularClient/vercel.json`, `AngularClient/package.json`, `Backend/PharmacyMS.API/Program.cs` and the setup/test outcomes described for the newer local copy. The GitHub `main` files inspected for this guide were not yet synchronized with those local changes. Before publishing the commercial ZIP, verify the actual packaged release contains:

1. `--provision-admin` CLI support, gated outside the normal web-service lifecycle.
2. Development-only default bootstrap and checked `AddToRoleAsync` results.
3. Correct and documented EF Core migrations, with no requirement to recreate `InitialCreate`.
4. No seller-owned API hostname in the *buyer's deployed* Vercel rewrite.
5. Tested deployment on the actual buyer/provider configuration.

**Deployment documentation is not a warranty, hosting-provider certification, or substitute for security testing.**
