namespace PharmacyMS.Infrastructure.Security;

public static class PermissionNames
{
    public static readonly string[] All = [
        "Medicines.Read","Medicines.Write","Categories.Read","Categories.Write",
        "Generics.Read","Generics.Write","Shelves.Read","Shelves.Write",
        "Suppliers.Read","Suppliers.Write","Customers.Read","Customers.Write",
        "Purchases.Read","Purchases.Write","Sales.Read","Sales.Write",
        "Stock.Read","Dashboard.Read","Reports.Read","Users.Manage","Roles.Manage"
    ];
}
