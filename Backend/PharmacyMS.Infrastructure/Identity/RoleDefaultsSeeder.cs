using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using PharmacyMS.Infrastructure.Data;
using PharmacyMS.Infrastructure.Security;

namespace PharmacyMS.Infrastructure.Identity;

// Deliberately create-only. Re-running this must NEVER restore revoked permissions.
public static class RoleDefaultsSeeder
{
    private static readonly IReadOnlyDictionary<string, string[]> Defaults =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["Pharmacist"] = [
                "Medicines.Read", "Medicines.Write", "Categories.Read", "Generics.Read",
                "Shelves.Read", "Suppliers.Read", "Customers.Read", "Purchases.Read",
                "Purchases.Write", "Sales.Read", "Sales.Write", "Stock.Read",
                "Dashboard.Read", "Reports.Read"
            ],
            ["Cashier"] = [
                "Medicines.Read", "Categories.Read", "Generics.Read", "Shelves.Read",
                "Customers.Read", "Customers.Write", "Sales.Read", "Sales.Write", "Stock.Read"
            ]
        };

    public static async Task<IReadOnlyList<string>> EnsureMissingRolesAsync(RoleManager<IdentityRole> roles)
    {
        var created = new List<string>();
        foreach (var (name, permissions) in Defaults)
        {
            if (await roles.RoleExistsAsync(name)) continue;
            var role = new IdentityRole(name);
            var result = await roles.CreateAsync(role);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Cannot create default role {name}: " +
                    string.Join("; ", result.Errors.Select(x => x.Description)));
            foreach (var permission in permissions)
            {
                if (!PermissionNames.All.Contains(permission, StringComparer.Ordinal))
                    throw new InvalidOperationException($"Unknown default permission {permission}.");
                result = await roles.AddClaimAsync(role, new Claim("Permission", permission));
                if (!result.Succeeded)
                    throw new InvalidOperationException($"Cannot seed {name}.{permission}: " +
                        string.Join("; ", result.Errors.Select(x => x.Description)));
            }
            created.Add(name);
        }
        return created;
    }
}
