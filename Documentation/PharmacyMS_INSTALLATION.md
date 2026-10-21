# PharmacyMS ERP — Installation Guide

**Version:** 1.0.0 (release candidate)  
**Technology:** ASP.NET Core 10 Web API, Angular, Entity Framework Core 10, Microsoft SQL Server  
**Audience:** Developers and technical buyers deploying their own copy of PharmacyMS ERP

> This guide describes the current source-code setup. The product should not be presented as certified production-ready until deployment, dependency licensing and security reviews are completed.

## 1. Requirements

For a Windows development installation:

- .NET 10 SDK
- Node.js and npm compatible with the included Angular version (the project was built successfully with Node.js 24.11.1 and npm 11.16.0)
- Microsoft SQL Server or SQL Server LocalDB
- EF Core CLI (`dotnet-ef`) compatible with the application's EF Core version
- PowerShell and a code editor

Use your own database, application credentials, and infrastructure. The buyer must not use the seller's live database, JWT key, or admin account.

## 2. Project layout

```text
PharmacyMS-ERP/
├── Backend/
│   ├── PharmacyMS.API/
│   ├── PharmacyMS.Application/
│   ├── PharmacyMS.Domain/
│   ├── PharmacyMS.Infrastructure/
│   ├── tests/
│   └── PharmacyMS.slnx
└── AngularClient/
    ├── src/
    ├── package.json
    └── package-lock.json
```

The domain layer contains entities and rules. The application layer contains use cases and contracts. Infrastructure implements EF Core and Identity. The API project exposes HTTP endpoints. Angular provides the web interface.

## 3. Restore and build the backend

Open PowerShell at the extracted project root:

```powershell
cd .\Backend
dotnet --version
dotnet restore .\PharmacyMS.slnx
dotnet build .\PharmacyMS.slnx -c Release
```

## 4. Configure an isolated SQL Server database

For **Windows LocalDB testing**, start the instance:

```powershell
sqllocaldb start MSSQLLocalDB
```

In the **same PowerShell session**, point the application to a new database:

```powershell
$env:ConnectionStrings__PharmacyConnection = 'Server=(localdb)\MSSQLLocalDB;Database=PharmacyMS_BuyerSetup_Test;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true'
```

Use a distinct database name. Do not reuse an existing production database. For other SQL Server installations, configure an appropriate connection string securely outside tracked source files. SQL Server LocalDB is Windows-only.

## 5. Configure JWT signing credentials

The API requires `Jwt:Key` to contain at least 32 bytes of signing-key material. Generate a fresh secret locally instead of copying a shared example:

```powershell
$bytes = New-Object byte[] 48
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
try {
    $rng.GetBytes($bytes)
    $env:Jwt__Key = [Convert]::ToBase64String($bytes)
}
finally {
    $rng.Dispose()
    [Array]::Clear($bytes, 0, $bytes.Length)
    Remove-Variable bytes -ErrorAction SilentlyContinue
}
```

Keep the JWT key in a secure secret manager or environment variable. The temporary session key above is suitable for validating installation, but must not be reused as a permanent multi-instance production configuration. Use consistent securely stored key material across application restarts in production.

## 6. Apply existing EF Core migrations

Install `dotnet-ef` if it is not already available:

```powershell
dotnet ef --version
```

If missing, install a version compatible with EF Core 10, then apply the **existing** migrations from the `Backend` folder:

```powershell
dotnet ef database update `
  --project .\PharmacyMS.Infrastructure `
  --startup-project .\PharmacyMS.API `
  --context ApplicationDbContext `
  --connection $env:ConnectionStrings__PharmacyConnection
```

**Do not run `dotnet ef migrations add InitialCreate` during normal installation.** Migration source files are already supplied. Creating a new initial migration may cause schema conflicts.

Confirm the migration command succeeds against the intended *new* database before continuing.

## 7. Provision the first Administrator

The current release candidate includes a one-time local command-line provisioning mode (`--provision-admin`). It creates the initial Administrator without exposing a public setup HTTP endpoint.

Set the application environment and choose an admin email:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Production'
$env:DOTNET_ENVIRONMENT = 'Production'
$env:Bootstrap__Email = 'administrator@example.com'
```

Set the initial password without typing it directly into the command history:

```powershell
$securePassword = Read-Host 'New administrator password' -AsSecureString
$ptr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
try {
    $env:Bootstrap__Password = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr)
}
finally {
    [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr)
    Remove-Variable securePassword -ErrorAction SilentlyContinue
}
```

Run provisioning from `Backend`:

```powershell
try {
    dotnet run `
      --project .\PharmacyMS.API\PharmacyMS.API.csproj `
      -c Release `
      --no-launch-profile `
      -- --provision-admin

    if ($LASTEXITCODE -ne 0) { throw 'Administrator provisioning failed.' }
}
finally {
    Remove-Item Env:Bootstrap__Email -ErrorAction SilentlyContinue
    Remove-Item Env:Bootstrap__Password -ErrorAction SilentlyContinue
}
```

**Expected:** `Admin provisioning completed.`

Provisioning an existing administrator again should not create a duplicate user. This behaviour has been checked against an isolated LocalDB test database. Only provision an account you control; do not run the command against a third-party database.

## 8. Start the backend API

Keep your configured connection string and JWT key available to the running API process. For a local test on a fixed port:

```powershell
dotnet run `
  --project .\PharmacyMS.API\PharmacyMS.API.csproj `
  -c Release `
  --no-launch-profile `
  --urls http://127.0.0.1:5080
```

Check:

- Health: `http://127.0.0.1:5080/api/health`
- Database readiness: `http://127.0.0.1:5080/api/health/ready`

Swagger is available only when the API runs in Development mode. For internet-facing deployment, use HTTPS and restrict administrative access appropriately.

## 9. Restore and build Angular

In a separate terminal, from the project root:

```powershell
cd .\AngularClient
npm ci
npm run build
```

The verified production output location is:

```text
AngularClient/dist/pharmacyms-angular-client/browser/
```

For local Angular development, use the project's configured development server and ensure `/api` requests are routed to the locally running backend. The committed production `vercel.json` rewrite may route `/api` to the seller's live demonstration service. **Replace that destination with your own backend before deploying your purchased copy.** Do not connect buyer production data to the seller's public API.

## 10. Validate installation

1. Start the API with the buyer's database and JWT configuration.
2. Confirm `/api/health/ready` returns success.
3. Open the Angular frontend configured for that API.
4. Sign in using the newly provisioned Administrator credentials.
5. Confirm medicines, customers, purchases, sales and stock views can be accessed according to role permissions.
6. Create fictional test inventory and test a purchase/sale flow in a disposable test database.
7. Run the automated backend test suite:

```powershell
cd .\Backend
dotnet test .\PharmacyMS.slnx -c Release
```

The release candidate most recently reported 4 passing xUnit domain tests. This is not a substitute for integration and deployment testing.

## 11. Remove temporary secrets

When finished with **temporary local test sessions**, clear the session variables:

```powershell
Remove-Item Env:Bootstrap__Email -ErrorAction SilentlyContinue
Remove-Item Env:Bootstrap__Password -ErrorAction SilentlyContinue
Remove-Item Env:Jwt__Key -ErrorAction SilentlyContinue
Remove-Item Env:ConnectionStrings__PharmacyConnection -ErrorAction SilentlyContinue
```

A production deployment still needs persistent credentials stored securely at the hosting provider or in a secret manager. Do not commit production secrets to `appsettings.json`, Git, ZIP archives or screenshots.

## 12. Common issues

| Symptom | Suggested check |
|---|---|
| `Configure Jwt:Key (at least 32 bytes)` | Set `Jwt__Key` in the environment for the process before starting the API or provisioning. |
| SQL connection error | Verify the targeted SQL Server instance, credentials, database name and connection string. |
| `dotnet ef` not found | Install a compatible EF Core CLI tool and restart the shell if needed. |
| `401 Unauthorized` | Confirm credentials and that the token was issued by the buyer's API with the matching signing key. |
| `403 Forbidden` | Confirm the authenticated user's assigned role and effective permissions. |
| Angular API calls target the wrong server | Check deployment rewrites and proxy settings for `/api`. |
| `dotnet test` reports only build success | The older Domain.Checks console utility is not a recognized xUnit test project; use the included Domain.Tests project. |

## 13. Production deployment checklist

Before serving real users, verify HTTPS, secure persistent key management, SQL backup/restore procedures, an appropriate production connection string, frontend API routing, role enforcement, logging, dependency vulnerabilities, applicable privacy requirements and commercial licenses. Keep demo and production databases separate.

---

**Documentation status:** English installation draft prepared from the repository's existing Bengali setup guide and the later test results. The exact commands should be retested against the final packaged release before distribution.
