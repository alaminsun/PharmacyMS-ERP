using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PharmacyMS.Application.Common;
using PharmacyMS.Application.DTOs;
using PharmacyMS.Application.Interfaces;
using PharmacyMS.Infrastructure.Data;
using PharmacyMS.Infrastructure.Security;

namespace PharmacyMS.Infrastructure.Identity;

public sealed class IdentityService(UserManager<ApplicationUser> users,RoleManager<IdentityRole> roles,
    SignInManager<ApplicationUser> signInManager,IConfiguration config) : IIdentityService
{
    public async Task<LoginResponse?> LoginAsync(LoginRequest input,CancellationToken ct)
    {
        var user=await users.FindByEmailAsync(input.Email);
        if (user is null || !user.IsActive || !user.EmailConfirmed) return null;
        var signedIn=await signInManager.CheckPasswordSignInAsync(user,input.Password,lockoutOnFailure:true);
        if (!signedIn.Succeeded) return null;
        var jwtKey=config["Jwt:Key"] ?? throw new InvalidOperationException("Configure Jwt:Key using user-secrets or environment variables.");
        if (Encoding.UTF8.GetByteCount(jwtKey)<32) throw new InvalidOperationException("JWT signing key must be at least 32 bytes.");
        var issuer=config["Jwt:Issuer"] ?? "PharmacyMS";
        var audience=config["Jwt:Audience"] ?? "PharmacyMS.Angular";
        var duration=int.TryParse(config["Jwt:ExpirationMinutes"],out var mins) && mins is >0 and <=1440 ? mins : 60;
        var expires=DateTime.UtcNow.AddMinutes(duration);
        var userRoles=await users.GetRolesAsync(user);
        var claims=new List<Claim>{
            new(ClaimTypes.NameIdentifier,user.Id),new(ClaimTypes.Email,user.Email ?? ""),
            new(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
            new("security_stamp",await users.GetSecurityStampAsync(user))
        };
        foreach(var role in userRoles) claims.Add(new Claim(ClaimTypes.Role,role));
        var token=new JwtSecurityToken(issuer,audience,claims,expires:expires,
            signingCredentials:new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),SecurityAlgorithms.HmacSha256));
        return new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token),expires,user.Email ?? input.Email,userRoles.ToArray());
    }

    public async Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken ct)
    {
        var source=await users.Users.OrderBy(x=>x.Email).ToListAsync(ct);
        var result=new List<UserDto>();
        foreach(var user in source) result.Add(new UserDto(user.Id,user.Email ?? "",user.IsActive,(await users.GetRolesAsync(user)).ToArray()));
        return result;
    }
    public async Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken ct)
    {
        var source=await roles.Roles.OrderBy(x=>x.Name).ToListAsync(ct);
        var result=new List<RoleDto>();
        foreach(var role in source)
        {
            var claims=await roles.GetClaimsAsync(role);
            result.Add(new RoleDto(role.Name ?? "",claims.Where(x=>x.Type=="Permission").Select(x=>x.Value).ToArray()));
        }
        return result;
    }
    public async Task<string> CreateUserAsync(CreateUserRequest input,CancellationToken ct)
    {
        if (await users.FindByEmailAsync(input.Email) is not null) throw new ApiException(409,"User already exists.");
        if (await roles.FindByNameAsync(input.Role) is null) throw new ApiException(400,"Role not found.");
        var user=new ApplicationUser { UserName=input.Email, Email=input.Email,EmailConfirmed=true,IsActive=true};
        var created=await users.CreateAsync(user,input.Password);
        if (!created.Succeeded) throw new ApiException(400,string.Join(" ",created.Errors.Select(x=>x.Description)));
        var assigned=await users.AddToRoleAsync(user,input.Role);
        if (!assigned.Succeeded)
        {
            await users.DeleteAsync(user);
            throw new ApiException(400,"Could not assign role.");
        }
        return user.Id;
    }
    public async Task AssignRoleAsync(AssignRoleRequest input,CancellationToken ct)
    {
        var user=await users.FindByEmailAsync(input.Email) ?? throw new ApiException(404,"User not found.");
        if (await roles.FindByNameAsync(input.Role) is null) throw new ApiException(404,"Role not found.");
        if (await users.IsInRoleAsync(user,input.Role)) return;
        var result=await users.AddToRoleAsync(user,input.Role);
        if (!result.Succeeded) throw new ApiException(400,"Could not assign role.");
        await RequireIdentitySuccess(users.UpdateSecurityStampAsync(user),"Could not invalidate old tokens.");
    }
    public async Task CreateRoleAsync(CreateRoleRequest input,CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.Name) || input.Name.Length>100) throw new ApiException(400,"Invalid role name.");
        if (await roles.RoleExistsAsync(input.Name)) throw new ApiException(409,"Role already exists.");
        var result=await roles.CreateAsync(new IdentityRole(input.Name));
        if (!result.Succeeded) throw new ApiException(400,"Could not create role.");
    }
    public async Task GrantPermissionAsync(GrantPermissionRequest input,CancellationToken ct)
    {
        if (!PermissionNames.All.Contains(input.Permission,StringComparer.Ordinal)) throw new ApiException(400,"Unknown permission.");
        // These are Admin-only capabilities, regardless of role claim contents.
        if (input.Permission is "Users.Manage" or "Roles.Manage")
            throw new ApiException(400,"User and role management permissions are reserved for Admin.");
        if (string.Equals(input.Role,"Admin",StringComparison.OrdinalIgnoreCase))
            throw new ApiException(400,"Admin is a protected role; permissions are inherited automatically.");
        var role=await roles.FindByNameAsync(input.Role) ?? throw new ApiException(404,"Role not found.");
        var existing=await roles.GetClaimsAsync(role);
        if (existing.Any(x=>x.Type=="Permission" && x.Value==input.Permission)) return;
        var result=await roles.AddClaimAsync(role,new Claim("Permission",input.Permission));
        if (!result.Succeeded) throw new ApiException(400,"Could not add permission.");
    }

    public async Task RevokePermissionAsync(RevokePermissionRequest input,CancellationToken ct)
    {
        if (!PermissionNames.All.Contains(input.Permission,StringComparer.Ordinal))
            throw new ApiException(400,"Unknown permission.");
        if (string.Equals(input.Role,"Admin",StringComparison.OrdinalIgnoreCase))
            throw new ApiException(400,"Admin is a protected role.");
        var role=await roles.FindByNameAsync(input.Role) ?? throw new ApiException(404,"Role not found.");
        var claims=(await roles.GetClaimsAsync(role))
            .Where(x=>x.Type=="Permission" && x.Value==input.Permission).ToArray();
        foreach (var claim in claims)
            await RequireIdentitySuccess(roles.RemoveClaimAsync(role,claim),"Could not revoke permission.");
        // Remove all matching claims, even if legacy data accidentally contains duplicates.
        // Handler checks role claims in DB on EVERY request: revocation takes effect immediately.
    }

    public async Task RemoveRoleAsync(RemoveRoleRequest input,CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.Email) || string.IsNullOrWhiteSpace(input.Role))
            throw new ApiException(400,"Email and role are required.");
        var user=await users.FindByEmailAsync(input.Email) ?? throw new ApiException(404,"User not found.");
        if (!await users.IsInRoleAsync(user,input.Role)) return;
        if (string.Equals(input.Role,"Admin",StringComparison.OrdinalIgnoreCase) && user.IsActive)
            await EnsureOtherActiveAdminAsync();
        await RequireIdentitySuccess(users.RemoveFromRoleAsync(user,input.Role),"Could not remove user role.");
        await RequireIdentitySuccess(users.UpdateSecurityStampAsync(user),"Could not invalidate old tokens.");
    }

    public async Task SetUserStatusAsync(SetUserStatusRequest input,CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.Email)) throw new ApiException(400,"Email is required.");
        var user=await users.FindByEmailAsync(input.Email) ?? throw new ApiException(404,"User not found.");
        if (user.IsActive==input.IsActive) return;
        if (!input.IsActive && user.IsActive && await users.IsInRoleAsync(user,"Admin"))
            await EnsureOtherActiveAdminAsync();
        user.IsActive=input.IsActive;
        await RequireIdentitySuccess(users.UpdateAsync(user),"Could not update user status.");
        await RequireIdentitySuccess(users.UpdateSecurityStampAsync(user),"Could not invalidate old tokens.");
    }

    public Task<IReadOnlyList<string>> SeedDefaultRolesAsync(CancellationToken ct)
        => RoleDefaultsSeeder.EnsureMissingRolesAsync(roles);

    public async Task<MyAccessDto> GetMyAccessAsync(string userId,CancellationToken ct)
    {
        var user=await users.FindByIdAsync(userId);
        if (user is null || !user.IsActive || !user.EmailConfirmed)
            throw new ApiException(401,"Account unavailable.");
        var names=await users.GetRolesAsync(user);
        if (names.Contains("Admin",StringComparer.OrdinalIgnoreCase))
            return new MyAccessDto(user.Id,user.Email ?? "",names.ToArray(),PermissionNames.All);
        var granted=new HashSet<string>(StringComparer.Ordinal);
        foreach (var name in names)
        {
            var role=await roles.FindByNameAsync(name);
            if (role is null) continue;
            foreach (var claim in await roles.GetClaimsAsync(role))
                if (claim.Type=="Permission" && PermissionNames.All.Contains(claim.Value,StringComparer.Ordinal)
                    && claim.Value is not "Users.Manage" and not "Roles.Manage")
                    granted.Add(claim.Value);
        }
        return new MyAccessDto(user.Id,user.Email ?? "",names.ToArray(),granted.OrderBy(x=>x).ToArray());
    }

    private async Task EnsureOtherActiveAdminAsync()
    {
        var admins=await users.GetUsersInRoleAsync("Admin");
        if (admins.Count(x=>x.IsActive)<=1)
            throw new ApiException(409,"Cannot remove or deactivate the last active Admin.");
    }

    private static async Task RequireIdentitySuccess(Task<IdentityResult> action,string message)
    {
        var result=await action;
        if (!result.Succeeded)
            throw new ApiException(400,message+" "+string.Join("; ",result.Errors.Select(x=>x.Description)));
    }
}
