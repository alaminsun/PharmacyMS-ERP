using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PharmacyMS.Infrastructure.Data;

namespace PharmacyMS.Infrastructure.Identity;

// Runs only in Development AND only when email/password are supplied. Never use shared defaults.
public static class AdminBootstrap
{
    public static async Task EnsureAsync(IServiceProvider services,IConfiguration config)
    {
        var email=config["Bootstrap:Email"];
        var password=config["Bootstrap:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return;
        using var scope=services.CreateScope();
        var roles=scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var users=scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        if (!await roles.RoleExistsAsync("Admin"))
        {
            var result=await roles.CreateAsync(new IdentityRole("Admin"));
            if(!result.Succeeded) throw new InvalidOperationException("Admin role creation failed.");
        }
        // Only create missing roles; never undo an administrator's permission revocations.
        await RoleDefaultsSeeder.EnsureMissingRolesAsync(roles);
        var user=await users.FindByEmailAsync(email);
        if (user is null)
        {
            user=new ApplicationUser{UserName=email,Email=email,EmailConfirmed=true,IsActive=true};
            var created=await users.CreateAsync(user,password);
            if(!created.Succeeded) throw new InvalidOperationException("Admin bootstrap failed: "+string.Join("; ",created.Errors.Select(e=>e.Description)));
            await users.AddToRoleAsync(user,"Admin");
        }
        else if(!await users.IsInRoleAsync(user,"Admin"))
        {
            // Never promote an existing account just because bootstrap variables were configured.
            throw new InvalidOperationException("Bootstrap email is already registered without Admin role. Configure a different email.");
        }
    }
}
