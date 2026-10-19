using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using PharmacyMS.Infrastructure.Data;

namespace PharmacyMS.API.Security;

public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission {get;} = permission;
}

// Permissions are read from the database on each request, so removing a role/permission takes effect immediately.
public sealed class PermissionAuthorizationHandler(UserManager<ApplicationUser> users,
    RoleManager<IdentityRole> roles) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,PermissionRequirement requirement)
    {
        var id=context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(id)) return;
        var user=await users.FindByIdAsync(id);
        if (user is null || !user.IsActive || !user.EmailConfirmed || await users.IsLockedOutAsync(user)) return;
        var names=await users.GetRolesAsync(user);
        if (names.Contains("Admin",StringComparer.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
            return;
        }
        if (requirement.Permission is Permissions.UsersManage or Permissions.RolesManage) return;
        foreach(var name in names)
        {
            var role=await roles.FindByNameAsync(name);
            if (role is null) continue;
            var claims=await roles.GetClaimsAsync(role);
            if (claims.Any(x=>x.Type=="Permission" && x.Value==requirement.Permission))
            {
                context.Succeed(requirement);
                return;
            }
        }
    }
}
