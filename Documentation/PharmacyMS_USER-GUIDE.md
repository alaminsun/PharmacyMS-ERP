# PharmacyMS — User Guide

**Version:** 1.0.0  
**Audience:** Pharmacy administrators, pharmacists, cashiers, and inventory staff  
**Product:** Pharmacy Inventory, Purchase & Sales Management System

> This guide describes the features available in PharmacyMS v1.0. Screen names and the buttons shown can vary slightly by deployment and role permissions. Your organization's administrator controls access. This is an inventory and sales management application, not a clinical decision-support or prescribing system.

## 1. Getting started

1. Open the PharmacyMS web address provided by your administrator.
2. Enter your assigned email address and password.
3. Select **Sign in**.
4. After authentication, use the left-hand navigation menu to open a module.
5. When finished, use **Sign out**, especially on a shared computer.

**If you cannot sign in:** Check your email and password, then contact your administrator to verify that the account is active. Do not share credentials with colleagues.

### Roles and access

PharmacyMS supports role-based access control (RBAC). The available modules depend on your assigned permissions.

| Typical role | Intended usage | Important note |
|---|---|---|
| Admin | User management, permissions, and overall administration | Protect administrator credentials. |
| Cashier | Customer management and sales; read-only access to selected catalog and stock information | Purchasing and administration may be hidden or blocked. |
| Pharmacist | Catalog, inventory and reporting tasks as configured | Exact access depends on the permission settings. |

The role names do not guarantee a fixed permission set: an administrator may customize role permissions.

## 2. Dashboard

The **Dashboard** provides a summary of operational information, subject to your permissions. Depending on available data, it may show medicine, customer and supplier counts, purchases, sales, and stock indicators.

Use it to review trends and identify items needing attention. Figures depend on the transactions entered into the system; they are not a substitute for physical inventory checks or accounting reconciliation.

## 3. Catalog and reference data

Before creating transactions, configure the data used throughout the application:

- **Categories:** Group medicines by category.
- **Generics:** Maintain generic medicine reference information.
- **Shelves:** Identify where products are stored.
- **Suppliers:** Record the suppliers used for purchasing.
- **Customers:** Maintain customer records where needed; walk-in sales may use a walk-in option.
- **Medicines:** Record the medicine code, name, strength, references, purchasing price, selling price, status and other available fields.

### Add a medicine

1. Open **Medicines**.
2. Choose the available **Add** or **Create** action.
3. Enter the medicine code, name and other required fields.
4. Select existing category, generic, shelf and supplier references as appropriate.
5. Enter purchase and sale prices carefully.
6. Save and check that the medicine appears in the catalog.

**Good practices:** Use unique medicine codes, review price changes before saving, and deactivate products that should no longer be sold instead of removing historical records. The exact editable fields depend on the installed version.

## 4. Purchases — receiving inventory

A **Purchase** records stock received from a supplier. Product quantities are tracked through stock batches.

### Record a purchase

1. Open **Purchases** (requires purchasing permission).
2. Start a new purchase.
3. Select a supplier and enter the invoice number or other required header information.
4. Add purchase lines by selecting a medicine.
5. For each line, enter its **batch number**, **expiry date**, **quantity**, and **unit cost**.
6. Review subtotals, discount (if used), and total.
7. Save the purchase, then check the resulting stock in **Stock Overview**.

**Important:** Check quantities, batch numbers, and expiry dates against the physical delivery *before* saving. The application creates historical purchase records; it should not be treated as a purchasing returns workflow.

### Example

| Field | Illustrative value |
|---|---|
| Invoice | `PUR-DEMO-001` |
| Supplier | A supplier already created in the system |
| Medicine | A medicine already in the catalog |
| Batch | `DEMO-BATCH-01` |
| Expiry | `2028-12-31` |
| Quantity | `10` |
| Unit cost | `8.50` |

Examples are fictional demonstration data, not recommended product pricing or real medication guidance.

## 5. Sales — recording a customer sale

A **Sale** deducts quantities from the selected available stock batches.

### Create a sale

1. Open **Sales**.
2. Start a new sale and select a customer if needed.
3. Select the medicine(s) to sell.
4. Review the available batch, expiry and quantity details in the sales interface.
5. Enter the requested quantity and confirm that sufficient valid stock exists.
6. Review the sale total and any permitted discount.
7. Save the sale; check the receipt or invoice view if your installation offers it.
8. Confirm that the quantity in **Stock Overview** has decreased as expected.

**Expiry and availability:** The backend is designed to reject expired or insufficient stock. The sales interface may help users choose batches in earliest-expiry order (FEFO). Users should still review the chosen batch before finalizing: do not assume a mandatory server-side FEFO allocation policy unless your deployment has been separately validated for it.

**Price source:** Selling prices are determined by the application's current catalog settings; review any changes before processing transactions.

## 6. Stock Overview

Use **Stock Overview** to view available stock, medicine batches and expiry-related information that your role permits.

A basic stock reconciliation procedure is:

1. Select a medicine in the stock listing.
2. Review its available quantities and applicable batches.
3. Compare the system figure with the physical count.
4. Review recent purchases and sales if a mismatch is found.
5. Escalate discrepancies to an administrator; do not invent transactions to hide them.

Low-stock information may depend on the minimum-stock value configured for each medicine. An empty or zero stock quantity does not necessarily mean the item has been deleted from the catalog.

## 7. Reports and receipts

Users with **Reports** permission can review available purchase and sales reporting for a selected time period.

1. Open **Reports**.
2. Set the date range or available filters.
3. Generate or refresh the report.
4. Check the reported total against saved purchase and sales records.
5. Where the interface supports it, print an invoice or use the browser's **Save as PDF** function.

**Scope:** These reports describe transactions in PharmacyMS. They are not a complete accounting ledger, tax filing system, supplier payment system, or customer-debt management module.

## 8. Administration — users and roles

Only authorized administrators should use administration features.

### Create a team member

1. Open **Administration → Team members**.
2. Select the action to add a user.
3. Enter their email and other required account details.
4. Assign an appropriate role, such as Cashier or Pharmacist.
5. Provide the initial password through a secure channel—not in public chat or shared documents.
6. Save, then ask the user to sign in and verify their permitted menus.

### Manage permissions

1. Open **Administration → Roles & permissions**.
2. Select an editable role.
3. Review the permissions for reading, writing or managing each module.
4. Make only the necessary changes and save.
5. Verify both the visible navigation and authorization of actual actions.

Hiding a menu is not a substitute for backend authorization. The backend should reject requests for permissions the account does not hold.

### Deactivate a user

Where account-status controls are available, an administrator can deactivate a user who no longer needs access. Verify the account can no longer sign in. Avoid using shared accounts when individual accountability is required.

## 9. Audit history

If you have access to **Administration → Audit history**, you can review application activity metadata.

- Review the event time (typically UTC), user/actor, HTTP action, API path and response status.
- Use **Actor email** or **API path** filters to locate relevant events.
- For example, filtering by `/api/sales` can help locate activity associated with sales requests.

An audit entry indicates an observed request; it does not by itself prove inventory accuracy or constitute an immutable financial audit ledger. System administrators should also maintain appropriate backups and operational monitoring.

## 10. Common problems

| Problem | What to check |
|---|---|
| Cannot sign in | Correct email/password, active account, and API availability. |
| Menu is missing | Your role may lack permission for the module. |
| Action returns `403 Forbidden` | The signed-in account is not authorized for that action. |
| Action returns `401 Unauthorized` | Your session may be invalid or expired; sign in again. |
| Medicine unavailable for sale | Check active status, stock quantity, selected batch and expiry. |
| Purchase or sale total seems incorrect | Review line quantities, prices and discounts, then compare saved transactions. |
| Data looks out of date | Refresh the relevant page and check for recent transactions. |
| Reports are empty | Review date filters, permissions and whether transactions exist in that period. |
| Website is unavailable | Contact the administrator to check frontend, API and database connectivity. |

## 11. Operational and data-safety guidelines

- Use strong, unique passwords; do not share administrator credentials.
- Enter only data your organization is entitled to collect and process.
- Verify medicine identity, batch, quantity and expiry against physical records.
- Make regular database backups and periodically test restoration.
- Confirm the local legal, accounting, privacy and pharmacy-operation requirements before using this system in a regulated business.
- Do not use the system as a substitute for professional judgment regarding medication suitability or dispensing requirements.

## 12. Features not included in the standard v1.0 scope

The current source-code scope should **not** be advertised as including a full general ledger, integrated supplier/customer payments, purchase/sales returns, SaaS subscription billing, immutable accounting audit, or automated jurisdiction-specific pharmacy compliance unless a later release implements and verifies those features.

## 13. Further help

- **Installation:** `Documentation/INSTALLATION.md`
- **Deployment:** `Documentation/DEPLOYMENT.md`
- **API reference:** See the backend endpoint documentation supplied with the source package.
- **Access problems:** Contact your organization's PharmacyMS administrator.

---

**Document status:** Buyer-facing functional guide for PharmacyMS ERP v1.0.0. It describes confirmed modules and intended workflows; verify every label and screenshot against the specific release build before publishing it commercially.
