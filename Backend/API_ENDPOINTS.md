# HTTP endpoints (all authenticated except health and login)

| Method | Path | Permission |
|---|---|---|
| POST | /api/auth/login | public |
| GET | /api/auth/me | logged in |
| GET | /api/health | public |
| GET | /api/health/ready | public (development probe) |
| GET, GET {id}, POST, PUT {id}, DELETE {id} | /api/medicines | Medicines.Read/Write |
| GET, GET {id}, POST, PUT {id}, DELETE {id} | /api/categories | Categories.Read/Write |
| GET, GET {id}, POST, PUT {id}, DELETE {id} | /api/generics | Generics.Read/Write |
| GET, GET {id}, POST, PUT {id}, DELETE {id} | /api/shelves | Shelves.Read/Write |
| GET, GET {id}, POST, PUT {id}, DELETE {id} | /api/suppliers | Suppliers.Read/Write |
| GET, GET {id}, POST, PUT {id}, DELETE {id} | /api/customers | Customers.Read/Write |
| GET, POST | /api/purchases | Purchases.Read/Write |
| GET, POST | /api/sales | Sales.Read/Write |
| GET | /api/stock | Stock.Read |
| GET | /api/dashboard | Dashboard.Read |
| GET | /api/reports?fromUtc=...&toUtc=... | Reports.Read |
| GET, POST | /api/admin/users | Admin (current DB role) |
| POST | /api/admin/users/assign-role | Admin (current DB role) |
| GET, POST | /api/admin/roles | Admin (current DB role) |
| GET | /api/admin/roles/permissions | Admin (current DB role) |
| POST | /api/admin/roles/grant-permission | Admin (current DB role) |

Reference deletes return a conflict when records are used. Medicine delete means deactivate rather than physically removing historical records.
