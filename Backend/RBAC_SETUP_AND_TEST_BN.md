# PharmacyMS Backend — RBAC Security Update (বাংলা)

এই ZIP-এ আগের Reports এবং Low Stock code-এর সঙ্গে Admin / Pharmacist / Cashier permission management আপডেট আছে।

## আগে যা করবে

1. তোমার SQL Server LocalDB database backup রাখবে। এই ZIP-এ তোমার LocalDB data বা JWT user-secret নেই।
2. ZIP extract করে `PharmacyMS.slnx` Visual Studio-তে খুলবে। NuGet Restore এবং **Build Solution** করবে।
3. আগের Low Stock migration apply না করে থাকলে, Visual Studio **Package Manager Console**-এ চালাবে:

   ```powershell
   Update-Database -Project PharmacyMS.Infrastructure -StartupProject PharmacyMS.API -Context ApplicationDbContext
   ```

   **নতুন migration তৈরি করতে হবে না।** Existing database drop/ensure-created করবে না।
4. API চালু করে Swagger থেকে `POST /api/auth/login`-এ বিদ্যমান Admin email/password দিয়ে JWT নেবে। Swagger-এ Authorize করবে।
5. `POST /api/admin/roles/seed-defaults` চালাবে। এটি **শুধু অনুপস্থিত** `Cashier` ও `Pharmacist` role তৈরি করে; আগে revoke করা permissions আবার grant করে না।

## Role/Permission workflow

| Endpoint | Role | কাজ |
|---|---|---|
| `GET /api/admin/roles` | Admin | Role এবং assigned permissions দেখাবে |
| `GET /api/admin/roles/permissions` | Admin | Available permission নাম দেখাবে |
| `POST /api/admin/roles` | Admin | নতুন role তৈরি |
| `POST /api/admin/roles/grant-permission` | Admin | Role-এ permission যোগ |
| `POST /api/admin/roles/revoke-permission` | Admin | Permission revoke |
| `POST /api/admin/roles/seed-defaults` | Admin | অনুপস্থিত default role তৈরি |
| `GET /api/admin/users` | Admin | User ও role দেখাবে |
| `POST /api/admin/users` | Admin | নতুন User তৈরি |
| `POST /api/admin/users/assign-role` | Admin | User-এ role যোগ |
| `POST /api/admin/users/remove-role` | Admin | User-এর role সরানো |
| `PUT /api/admin/users/status` | Admin | Active/Inactive পরিবর্তন |
| `GET /api/auth/me/permissions` | Logged-in user | নিজের effective DB permissions |

### Sample JSON

`POST /api/admin/users`

```json
{"email":"cashier.test@example.com","password":"REPLACE_WITH_YOUR_OWN_STRONG_PASSWORD","role":"Cashier"}
```

`POST /api/admin/roles/grant-permission`

```json
{"role":"Cashier","permission":"Dashboard.Read"}
```

`POST /api/admin/roles/revoke-permission`

```json
{"role":"Cashier","permission":"Dashboard.Read"}
```

`POST /api/admin/users/remove-role`

```json
{"email":"cashier.test@example.com","role":"Cashier"}
```

`PUT /api/admin/users/status`

```json
{"email":"cashier.test@example.com","isActive":false}
```

### Expected authorization

- No JWT: **401 Unauthorized** for protected endpoints.
- Valid Cashier JWT on `GET /api/medicines`: **200 OK**.
- Valid Cashier JWT on `GET /api/admin/users`: **403 Forbidden**.
- Revoke Cashier `Medicines.Read`, same JWT: **403 Forbidden** at `GET /api/medicines` (DB role claims checked every request).
- Remove user role or deactivate user: old JWT becomes **401 Unauthorized** (security stamp / account status checked on each request).
- Account inactive: login **401 Unauthorized**. Reactivate and log in again to obtain a new JWT.
- `Users.Manage` and `Roles.Manage` are **Admin-only**, even when a non-Admin role has legacy claims. Granting these to non-Admin roles is blocked.
- Cannot remove/deactivate the last active Admin.

**সতর্কতা:** Role সরানো বা User নিষ্ক্রিয় করার আগে যে Admin JWT ব্যবহার করছো তা যেন শেষ সক্রিয় Admin-এর না হয়।

## Automated test — Windows PowerShell

Visual Studio-তে build/migration করার পরে API চালু করো। Solution root থেকে:

```powershell
.\tests\PharmacyMS.API.SmokeTests\Test-RbacAndReports.ps1 -BaseUrl "http://127.0.0.1:5080"
```

Script interactive-ভাবে Admin credentials এবং 12+ character strong test password চাইবে। **Local test database-এ নতুন Cashier, Pharmacist, Probe user এবং Probe role যোগ করবে।** Script শেষে এগুলো database-এ থেকে যাবে—কেবল development database-এ চালাবে। Production-এ চালাবে না।

Script `401/403`, role revocation, account deactivation/reactivation, Sales report math, Dashboard এবং Low Stock count পরীক্ষা করে। Fail হলে console থেকে error message কপি করে পাঠাও।

## Sales total 200 vs 160

`GET /api/reports/sales-details?fromUtc=2026-10-01T00:00:00Z&toUtc=2026-11-01T00:00:00Z`

এখানে প্রতিটি sale-এর historical `unitPrice`, `quantity`, `lineTotal`, `discount`, `grandTotal` দেখা যায়। 20 units x selling price 10 = 200 ঠিক আছে; 20 x selling price 8 = 160। Purchase unit price বিক্রয় মূল্য নয়। আগের sales data কোনোভাবেই rewrite করা হয়নি।

## Testing limitation

এই environment-এ .NET 10 SDK ও SQL Server LocalDB নেই। তাই প্রকৃত `dotnet build`, integration test, এবং migration execute করা সম্ভব হয়নি। Files ও endpoint-to-service wiring স্থিতিগতভাবে যাচাই হয়েছে; তোমার Windows machine-এ Build + Swagger/PowerShell test দিয়ে নিশ্চিত করবে।
