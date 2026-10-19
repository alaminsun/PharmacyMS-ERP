export interface LoginResponse { accessToken: string; expiresAtUtc: string; email: string; roles: string[]; }
export interface MyAccess { userId: string; email: string; roles: string[]; permissions: string[]; }
export interface Dashboard { medicines: number; suppliers: number; customers: number; availableUnits: number; expiredUnits: number; totalSales: number; totalPurchases: number; lowStockMedicines: number; outOfStockMedicines: number; }
export interface Medicine { id: number; code: string; name: string; strength: string | null; categoryId: number | null; categoryName: string | null; genericId: number | null; genericName: string | null; shelfId: number | null; shelfName: string | null; supplierId: number | null; supplierName: string | null; buyingPrice: number; sellingPrice: number; minimumStockLevel: number; isActive: boolean; }
export interface StockBatch { stockBatchId: number; medicineId: number; medicineCode: string; medicineName: string; batchNumber: string; expiryDate: string; quantityOnHand: number; isExpired: boolean; }
export interface LowStock { medicineId: number; medicineCode: string; medicineName: string; availableUnits: number; minimumStockLevel: number; status: 'LowStock' | 'OutOfStock'; }
export interface Transaction { id: number; invoiceNumber: string; supplierId?: number; customerId?: number | null; purchasedAtUtc?: string; soldAtUtc?: string; grandTotal: number; lineCount: number; }
export interface PurchaseLineInput { medicineId: number; batchNumber: string; expiryDate: string; quantity: number; unitPrice: number; }
export interface SaleLineInput { stockBatchId: number; quantity: number; }
export interface Report { fromUtc: string; toUtc: string; saleCount: number; purchaseCount: number; salesTotal: number; purchasesTotal: number; salesSubtotal: number; salesDiscount: number; purchasesSubtotal: number; purchasesDiscount: number; }
export interface SaleReportLine { stockBatchId: number; medicineCode: string; medicineName: string; quantity: number; unitPrice: number; lineTotal: number; }
export interface SaleReportDetail { saleId: number; invoiceNumber: string; soldAtUtc: string; subtotal: number; discount: number; grandTotal: number; items: SaleReportLine[]; }
export interface TeamUser { id: string; email: string; isActive: boolean; roles: string[]; }
export interface TeamRole { name: string; permissions: string[]; }
export interface Field { key: string; label: string; type?: 'text' | 'email' | 'number' | 'checkbox' | 'select' | 'textarea'; required?: boolean; options?: string; defaultValue?: unknown; }
export interface CatalogDefinition { key: string; title: string; endpoint: string; permission: string; columns: string[]; fields: Field[]; }
export type Item = Record<string, any>;

export interface PageResult<T> {items:T[];totalCount:number;page:number;pageSize:number;}
export interface ReceiptLine {medicineName:string;medicineCode:string;batchNumber:string;quantity:number;unitPrice:number;lineTotal:number;}
export interface Receipt {kind:string;id:number;invoiceNumber:string;dateUtc:string;partyName:string;subtotal:number;discount:number;grandTotal:number;items:ReceiptLine[];}
export interface AuditEvent {id:number;occurredAtUtc:string;actorEmail:string;action:string;path:string;statusCode:number;traceId:string;}
