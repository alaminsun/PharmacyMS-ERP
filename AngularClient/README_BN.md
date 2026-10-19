# PharmacyMS Angular 21 Client — বাংলা Setup Guide

এটি তোমার আপলোড করা `pharmacyms-client.zip`-এর সবুজ Pharmacy Workspace visual design ব্যবহার করে পুনর্নির্মিত **Angular standalone client**। পুরোনো project একটি Node/Vanilla JS client ছিল; এটি আলাদা Angular project। Backend হিসেবে ইতিমধ্যে প্রস্তুত **PharmacyMS ASP.NET Core Web API** ব্যবহার করে।

## Requirements

- Node.js **24.11.1** (তোমার ইনস্টল করা version), npm 11.16.0
- Angular CLI 21.2.13 (তোমার বর্তমান version)
- Angular 21.2.13, RxJS 7.8.x, TypeScript 5.9.x
- PharmacyMS.API running on `http://127.0.0.1:5080` (Swagger at `/swagger`)
- Backend migrations completed; an existing Admin user. 

**Version choice:** Angular 22 is newer, but Angular 22 requires Node 24.15.0+ (or compatible 22.22.3+, 26+). তোমার Node 24.11.1-এর সঙ্গে compatible রাখতে Angular 21.2.13 ব্যবহার করেছি। ভবিষ্যতে Node আপডেট করে Angular 22-এ upgrade করতে পারো।

## Windows Powershell — step-by-step

1. ZIP extract করে **`pharmacyms-angular21`** folder-এ PowerShell খোলো (Windows System32-তে command চালাবে না)।
2. Visual Studio-তে **PharmacyMS.API** project start করো। `/swagger` ঠিকভাবে খুলছে কি না দেখো।
3. Angular folder-এ:

```powershell
cd C:\YOUR_FOLDER\pharmacyms-angular21
npm install
npm start
```

4. Browser: **http://127.0.0.1:4200**
5. আগের Admin login credentials দিয়ে sign in করো।

Angular dev server-এর **`proxy.conf.json`** `/api` request তোমার .NET API-তে পাঠায়। CORS setting পরিবর্তন করতে হবে না; browser same-origin `/api` call করবে। API অন্য port-এ থাকলে `proxy.conf.json`-এর `target` সংশোধন করো।

## Files

- `src/app/core/auth.service.ts` — JWT, session state, DB effective permissions
- `src/app/core/auth.interceptor.ts` — `Authorization: Bearer` header, 401 handling
- `src/app/app.routes.ts` — authenticated/permission route guards
- `src/app/core/catalog.ts` — actual medicine/category/generic/shelf/supplier/customer fields
- `src/app/pages/` — Login, Dashboard, all catalog CRUD, Stock, Purchase, Sales, Reports, Users and Roles
- `src/styles.css` — uploaded visual design, preserved with small Angular adaptations
- `proxy.conf.json` — local `/api` forward to port 5080

## Backend integration

| Frontend page | Backend endpoint(s) | Permission |
|---|---|---|
| Login | POST `/api/auth/login`, GET `/api/auth/me/permissions` | authenticated |
| Dashboard | GET `/api/dashboard`, `/api/stock/low-stock` | Dashboard.Read, Stock.Read |
| Catalog CRUD | `/api/medicines`, `/api/categories`, `/api/generics`, `/api/shelves`, `/api/suppliers`, `/api/customers` | Module.Read / Module.Write |
| Purchases | GET/POST `/api/purchases` | Purchases.Read / Purchases.Write |
| Sales | GET/POST `/api/sales` | Sales.Read / Sales.Write |
| Stock | GET `/api/stock`, GET `/api/stock/low-stock` | Stock.Read |
| Reports | GET `/api/reports`, GET `/api/reports/sales-details` | Reports.Read |
| Users | GET/POST `/api/admin/users`, POST `assign-role`, `remove-role`, PUT `status` | Users.Manage |
| Roles | GET/POST `/api/admin/roles`, GET `/permissions`, POST `grant-permission`, `revoke-permission` | Roles.Manage |

### Important behavior

- Report date range: Backend uses `[fromUtc, toUtc)`. UI includes the chosen end date by sending midnight of the **next day in UTC** for `toUtc`.
- Dashboard: Backend `totalSales`, `totalPurchases`, `lowStockMedicines`, `outOfStockMedicines` (not the obsolete `salesTotal`, `purchasesTotal`, `lowStock[]`).
- Purchases: uses `medicineId`, `batchNumber`, `expiryDate`, `quantity`, `unitPrice`; Sale: sends `stockBatchId`, `quantity` **without trusting client pricing**.
- Existing Role/Permission storage stays inside SQL Server; Angular doesn't seed default roles automatically.
- Admin restrictions enforced server-side; UI hiding is not a security boundary.
- JWT in `sessionStorage`: survives refresh inside same tab, ends when session storage is cleared. Production deployments should review XSS protections/CSP and consider server-managed HttpOnly authentication via an appropriately designed BFF before handling sensitive real-world data. There is no `/api/auth/logout` or `/api/auth/csrf` endpoint in the current backend.

## Validation

```powershell
npm test
npm run typecheck
npm run build
```

`npm test` provides dependency-free helper/contract smoke tests. Build and backend integration require installing dependencies and running .NET API. If `npm install` fails check npm internet access and run in the Angular folder.

## Recommended manual checks

1. Admin login, dashboard values and `/stock/low-stock`.
2. Create/edit each catalog item; check role-based buttons with Pharmacist and Cashier.
3. Create a test purchase; verify batch stock; create sale; verify deduction.
4. Report for the transaction dates: subtotal, discount, net SalesTotal and saved line unit prices.
5. As Admin: create role, grant/revoke permission, create user, assign/remove role, change status. As Cashier: admin pages should be unavailable; direct Admin API returns 403.

### Limitations of supplied backend

- Backend offers **GET and POST only** for Sales/Purchases; client does not invent Update/Delete transactions.
- User fields are only `email/password/role` on create; backend does not have first/last name endpoints.
- Backend role modification is performed via individual Grant/Revoke calls. If network fails midway, some changes may have been saved—reload role before retrying.

**Built-in note:** The source package was prepared and statically validated here. An actual Angular `ng build` / .NET integration test could not run in the preparation environment without fetching Angular packages from npm; run the commands above locally and share errors for adjustments.


## নতুন: Toast Notification System

Angular 21-এর জন্য নিজস্ব dependency-free `ToastService` যোগ করা হয়েছে; `ngx-toastr` বা অন্য package install করতে হবে না।

- **Success (সবুজ):** Login, Medicine/Category/Generic/Shelf/Supplier/Customer Create/Update/Delete, Purchase, Sale, Create User, Assign/Remove Role, Activate/Deactivate Account, Role Permission Save, CSV Export।
- **Error (লাল):** যেকোনো `/api/...` HTTP error-এ এক জায়গা থেকে; `401`, `403`, validation error ও network unavailable—সবগুলো `errorMessage()` দিয়ে user-friendly করা হয়েছে। আগের inline error detail-ও থাকবে।
- **Warning (হলুদ):** Client-side invalid transaction বা invalid report date, এবং partial role update-এর সতর্কতা।
- **Info (নীল):** Sign out এবং manual Refresh।
- Toast উপরের ডান পাশে (mobile-এ responsive), সর্বোচ্চ ৪টি, ৪.৫–৭.৫ সেকেন্ডের মধ্যে auto-dismiss; `×` চাপলেও বন্ধ হয়। Screen-reader status/alert support আছে।

কীভাবে চালাবে (Angular-এর project folder থেকে):

```powershell
npm install
npm test
npm run build
npm start
```

Backend আগের মতো `http://127.0.0.1:5080`-এ চলবে; ভিন্ন port হলে `proxy.conf.json` বদলাবে।

**Smoke test:** Admin Login → success toast; Medicine Create/Update/Delete → success toast; invalid API call → red error toast; Purchase/Sale → transaction success toast; Assign/Revoke Permission → success toast; Reports CSV → export success toast।

> এখানে Angular packages fetch করা যায়নি বলে `ng build` চালানো হয়নি। Node-based source contract checks ও TypeScript-এর dependency stub check পাস করেছে, তবে Windows-এ প্রকৃত Angular compiler validation আবশ্যক।
