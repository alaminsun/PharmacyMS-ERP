# PharmacyMS Backend Corrections — Reports + Inventory Alerts

## What changed

1. **Financial report audit** (`GET /api/reports`)
   - Existing `salesTotal` / `purchasesTotal` still return **stored NET transaction totals** (`GrandTotal`). Historical financial data is NOT changed.
   - New fields: `salesSubtotal`, `salesDiscount`, `purchasesSubtotal`, `purchasesDiscount`.
   - A new endpoint, `GET /api/reports/sales-details`, lists each saved sale's items, quantities, historical `unitPrice`, `lineTotal`, `discount` and `grandTotal`.
   - **Why the total might be 200**: when the sale had 20 units at a medicine's stored **SellingPrice = 10**, the persisted total was 200. The *purchase unit price* (e.g. 5) is unrelated to the sale total. To get 160 for a NEW sale with 20 units and zero discount, the selling price must be 8 when that sale is created. DO NOT change historical SaleLine values to force 160.

2. **Inventory alerts**
   - Medicine entity/API now includes optional-on-input `minimumStockLevel` (defaults to 10, 0 allowed).
   - Dashboard includes `lowStockMedicines` and `outOfStockMedicines` counts.
   - `GET /api/stock/low-stock` returns medicine ID/code/name, remaining usable units, threshold and `status` (`LowStock` or `OutOfStock`). It includes active medicines that never had a stock batch.
   - Expired and expiring-today stock is excluded from usable quantity, and an inactive medicine is omitted from alerts.
   - Business rules: available=0 -> out of stock; 0<available<=minimum -> low stock.

3. **Code-First database migration**
   - `20261018020000_AddMedicineMinimumStockLevel` adds a non-null integer column to `Medicines`, default 10, with non-negative check constraint; existing records and transactions are retained.
   - EF Core model snapshot is updated for subsequent migrations.

## How to apply on your Windows / Visual Studio machine

1. **Back up the LocalDB database** before migration. Keep your existing database; DO NOT delete it or rerun InitialCreate manually.
2. Close the running API, open `PharmacyMS.slnx` in Visual Studio, restore NuGet packages and build.
3. In **Package Manager Console**, run:

   ```powershell
   Update-Database -Project PharmacyMS.Infrastructure -StartupProject PharmacyMS.API -Context ApplicationDbContext
   ```

   Or from the solution root with dotnet-ef installed:

   ```powershell
   dotnet ef database update --project PharmacyMS.Infrastructure --startup-project PharmacyMS.API --context ApplicationDbContext
   ```

   The migration is ALREADY INCLUDED — do NOT run `Add-Migration AddMedicineMinimumStockLevel` again.
4. Run the API with your existing JWT user secrets. Swagger: authenticate as Admin.

## Swagger tests

1. `GET /api/medicines` — verify a medicine now has `minimumStockLevel` (existing entries default to 10).
2. `PUT /api/medicines/{id}` — pass ALL the existing medicine fields, plus `"minimumStockLevel": 100` to create a low-stock alert where stock is 80.
3. `GET /api/dashboard` — `lowStockMedicines` should increase, `outOfStockMedicines` includes active medicines with no usable stock.
4. `GET /api/stock/low-stock` — look for a medicine with `availableUnits: 80`, `minimumStockLevel: 100`, `status: "LowStock"`.
5. `GET /api/reports?fromUtc=2026-10-01T00:00:00Z&toUtc=2026-11-01T00:00:00Z` — check `salesSubtotal - salesDiscount == salesTotal` and analogous purchase amounts.
6. `GET /api/reports/sales-details?fromUtc=2026-10-01T00:00:00Z&toUtc=2026-11-01T00:00:00Z` — locate your invoice and inspect its **historical** `unitPrice`, quantity and total. Note the upper bound `toUtc` is EXCLUSIVE.
7. Confirm `GET /api/stock?includeZero=false`, Purchase, Sale and login still work.

## Important limitations

- The ZIP is your source code; it does **not** contain your running Windows SQL Server LocalDB database or user secrets. It cannot automatically modify transactions that are already saved in your machine's database.
- No .NET 10 SDK/SQL Server LocalDB is present in the file-preparation environment, so a real `dotnet build`, migration execution, or Swagger integration test **could not be run here**. Build and migration must be run on Windows, and any errors can be reported back for follow-up.
- Reports use database-stored invoice GrandTotal. If your posted 200 came from a unit price of 10, 200 is mathematically correct and the code should **not** silently rewrite it to 160.
