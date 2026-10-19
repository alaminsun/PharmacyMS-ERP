# PharmacyMS — OmniStock-style Clean Architecture (Backend Only)

**ASP.NET Core 10 + SQL Server + Entity Framework Core Code-First + MediatR + FluentValidation + ASP.NET Core Identity/JWT.**

This backend was authored by studying the supplied `OmniStock.zip` structure: four class library/API projects, `Domain/Entities`, `Application/Features/{Feature}/Commands|Queries`, `IApplicationDbContext`, `Infrastructure/Data/ApplicationDbContext`, `IEntityTypeConfiguration<T>` and Controllers dispatching through MediatR.

No Dapper. No database-first scaffolding. No Angular frontend in this package.

`appsettings.json` intentionally contains no production connection string; `appsettings.Development.json` explicitly targets LocalDB, and JWT key is supplied through user secrets.

## Projects

- `PharmacyMS.API` — controllers, JWT, permission authorization, middleware, Swagger, CORS.
- `PharmacyMS.Application` — MediatR commands/queries, DTOs, validation, interfaces.
- `PharmacyMS.Domain` — catalog, stock, purchase/sale entities, stock rules.
- `PharmacyMS.Infrastructure` — `ApplicationDbContext : IdentityDbContext`, EF Core Fluent API configurations, Identity service, initial development admin bootstrap.
- `tests/PharmacyMS.Domain.Checks` — no-extra-dependency domain checks.

## Database warning

This is a **new EF Code-First schema**. It does NOT map the existing legacy `PharmacyAppDev` tables. The default development connection targets `PharmacyMSCodeFirstDev`, leaving existing `PharmacyAppDev` untouched. Old data requires a separate, carefully tested migration plan.

## Features implemented

- JWT login via Identity, admin-only user creation, role creation and permission grants.
- Medicine create/list/get/update/**deactivate**.
- Category/Generic/Shelf/Supplier/Customer CRUD.
- Purchase creation with batch stock increments, invoice uniqueness and transaction.
- Sale creation with batch stock deductions, expiry validation, optimistic rowversion, serializable transaction and server-side prices.
- Stock batches view, dashboard, date-bounded sales/purchase reports.
- Global error handling, validation, DB connection readiness probe, Swagger.

## Before running

Read `SETUP_BN.md`. Requires .NET 10 SDK and LocalDB or SQL Server; EF Core migrations are generated on **your Windows development computer**, not in this ZIP. Do not use `database/001_baseline.sql` from the old project.

This package is source-prepared and was **not** compiled or database-tested in this container (no .NET SDK / SQL Server). Do `dotnet build` and `dotnet ef ...` on Windows and share any errors. This is a learning/development foundation, not a production-validated pharmacy deployment.
