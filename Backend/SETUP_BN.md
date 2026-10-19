# PharmacyMS — বাংলায় Backend Setup (Code-First)

## প্রথমে যা বুঝবে

**OmniStock-এর মতো চারটি project:**

```
PharmacyMS/
  PharmacyMS.API/
    Controllers/ AuthController, MedicinesController, CategoriesController, ...
    Security/ Permissions, HasPermissionAttribute
    Exceptions/ GlobalExceptionHandler
    Program.cs
    appsettings.json
    appsettings.Development.json
  PharmacyMS.Application/
    Features/Medicines/Commands, Queries
    Features/Purchases/Commands, Queries
    Features/Sales/Commands, Queries
    Features/Auth/Commands, Queries
    DTOs/
    Interfaces/IApplicationDbContext.cs
    Behaviors/ValidationBehavior.cs
  PharmacyMS.Domain/
    Entities/
    Rules/
  PharmacyMS.Infrastructure/
    Data/ApplicationDbContext.cs
    Data/Configurations/*.cs
    Identity/
    Security/
  tests/
  PharmacyMS.slnx
```

`Domain Entity` = database/design model; `DTO` = request/response contract; `Controller` = HTTP; `Handler` = use case; `DbContext` = EF Core access.

**পুরোনো PharmacyAppDev database বদলাবে না।** এখানে `PharmacyMSCodeFirstDev` নামে *আলাদা, নতুন database* তৈরি হবে। পুরোনো table/data automatically convert বা import হবে না।

## 1. Extract করো

নতুন ZIP Extract করে উদাহরণস্বরূপ `D:\Sun\PharmacyMS-CodeFirst` ফোল্ডার তৈরি করো। ওই folder-এ PowerShell খোলো।

## 2. SDK / package restore

```
dotnet --version
# .NET 10 SDK installed হওয়া প্রয়োজন
dotnet restore .\PharmacyMS.slnx
dotnet build .\PharmacyMS.slnx
```

## 3. SQL Server LocalDB

```
SqlLocalDB start MSSQLLocalDB
```

`PharmacyMS.API/appsettings.Development.json`-এ `DefaultConnection` local instance ও `PharmacyMSCodeFirstDev`-এ point করছে। এটা password-less Windows Authentication ব্যবহার করে। Production-এ credentials JSON-এ commit করবে না।

## 4. JWT secret user-secrets দিয়ে দাও

Project root PowerShell:

```
dotnet user-secrets set "Jwt:Key" "ReplaceThisWithYourOwnRandomSecretAtLeast32Bytes" --project .\PharmacyMS.API\PharmacyMS.API.csproj
```

Example string কপি না করে নিজে unpredictable secret তৈরি করো; development-এর জন্য PowerShell-এ `System.Security.Cryptography.RandomNumberGenerator` দিয়েও generate করা যায়।

## 5. EF Core Code-First Migration

**`dbcontext scaffold` নয়।** এখানে entity class প্রথমে C#-এ লেখা হয়েছে।

```
dotnet tool install --global dotnet-ef --version 10.0.11
```

যদি already installed হয়, `dotnet tool update --global dotnet-ef --version 10.0.11` চালাও। এরপর project root থেকে:

```
dotnet ef migrations add InitialCreate `
  --project .\PharmacyMS.Infrastructure\PharmacyMS.Infrastructure.csproj `
  --startup-project .\PharmacyMS.API\PharmacyMS.API.csproj `
  --context ApplicationDbContext `
  --output-dir Migrations
```

Migration সফল হলে:

```
dotnet ef database update `
  --project .\PharmacyMS.Infrastructure\PharmacyMS.Infrastructure.csproj `
  --startup-project .\PharmacyMS.API\PharmacyMS.API.csproj `
  --context ApplicationDbContext
```

Migration চালানোর সময়ে `ASPNETCORE_ENVIRONMENT=Development` নিশ্চিত করো (command-এর আগে `$env:ASPNETCORE_ENVIRONMENT='Development'`)। **পুরোনো SQL script apply করবে না।** Database create হবে C# entity ও Fluent API config থেকে।

## 6. প্রথম Admin তৈরি (Development only)

PowerShell terminal-এ (নিজের strong password দাও):

```
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:Bootstrap__Email = 'admin@pharmacy.local'
$env:Bootstrap__Password = 'Replace-With-Your-Own-Strong-Password-123!'
dotnet run --project .\PharmacyMS.API\PharmacyMS.API.csproj --no-launch-profile -- --urls http://127.0.0.1:5080
```

`--urls` যদি project version-এ argument parse issue করে, `dotnet run --project ... --no-launch-profile -- --urls http://127.0.0.1:5080` ব্যবহার করো। Seed শুধু Development-এ হয়; seed user না থাকলে তৈরি হয়। Server বন্ধ করে PowerShell-এ variables সরাও:

```
Remove-Item Env:Bootstrap__Email, Env:Bootstrap__Password -ErrorAction SilentlyContinue
```

Production startup-এ bootstrap কখনো চালাবে না। `Jwt:Key` secret user-secrets-এ থাকে।

## 7. Swagger / Login

- Swagger: http://127.0.0.1:5080/swagger
- Health: http://127.0.0.1:5080/api/health
- Database ready: http://127.0.0.1:5080/api/health/ready

Swagger `POST /api/auth/login` JSON:

```json
{
  "email": "admin@pharmacy.local",
  "password": "YOUR_PASSWORD"
}
```

Response `accessToken` copy -> Swagger **Authorize**: `Bearer <accessToken>` (some Swagger versions prepend Bearer automatically; enter only token if requested). Then test `GET /api/medicines`.

## 8. CRUD pattern উদাহরণ

Medicine Create:

```json
{
  "code": "MED001", "name": "Sample Medicine", "strength": "100 mg",
  "categoryId": null, "genericId": null, "shelfId": null, "supplierId": null,
  "buyingPrice": 10.00, "sellingPrice": 12.50, "isActive": true
}
```

`POST /api/medicines` -> `CreateMedicineCommand` -> `CreateMedicineHandler` -> `IApplicationDbContext` -> EF Core `ApplicationDbContext` -> SQL Server. Dapper নেই।

Purchase (`POST /api/purchases`):

```json
{
  "invoiceNumber": "PUR-001", "supplierId": 1, "notes": "First delivery", "discount": 0,
  "items": [{"medicineId": 1, "batchNumber": "B001", "expiryDate": "2028-12-31", "quantity": 10, "unitPrice": 8.50}]
}
```

Sale (`POST /api/sales`): `GET /api/stock` থেকে `stockBatchId` নিয়ে পাঠাতে হবে:

```json
{
  "invoiceNumber": "SAL-001", "customerId": null, "notes": null, "discount": 0,
  "items": [{"stockBatchId": 1, "quantity": 2}]
}
```

Selling price server-side Medicine entity থেকে আসে। Expired medicine এবং insufficient stock reject হয়। Purchase/Sale **append-only**; কোনো historical invoice update/delete endpoint নেই।

## 9. Tests

```
dotnet run --project .\tests\PharmacyMS.Domain.Checks\PharmacyMS.Domain.Checks.csproj
```

## সতর্কতা

- EF migrations ZIP-এর মধ্যে pre-generated নেই; Windows-এ first migration create করতে হবে।
- Existing SQL schema থেকে automatically migrate করবে না। Previous PharmacyAppDev untouched.
- This source has not been compiled here: SDK এবং LocalDB environment-এ না থাকায় `dotnet build`/real login/DB integration যাচাই বাকি।
- Angular পরে; currently CORS future Angular localhost:4200-এর জন্য সেট আছে।
