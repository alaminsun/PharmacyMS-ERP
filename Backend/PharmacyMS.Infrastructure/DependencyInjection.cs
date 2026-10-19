using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PharmacyMS.Application.Interfaces;
using PharmacyMS.Infrastructure.Data;
using PharmacyMS.Infrastructure.Identity;

namespace PharmacyMS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,IConfiguration configuration)
    {
        var connection=configuration.GetConnectionString("PharmacyConnection");
        if (string.IsNullOrWhiteSpace(connection))
            throw new InvalidOperationException("Configure ConnectionStrings:PharmacyConnection; see appsettings.Development.json.");
        services.AddDbContext<ApplicationDbContext>(options=>options.UseSqlServer(connection));
        services.AddScoped<IApplicationDbContext>(sp=>sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IIdentityService,IdentityService>();
        services.AddIdentityCore<ApplicationUser>(options=>{
            options.Password.RequiredLength=12;
            options.Password.RequireDigit=true;
            options.Password.RequireUppercase=true;
            options.Password.RequireLowercase=true;
            options.Password.RequireNonAlphanumeric=true;
            options.User.RequireUniqueEmail=true;
            options.Lockout.DefaultLockoutTimeSpan=TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts=5;
        }).AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>()
          .AddSignInManager().AddDefaultTokenProviders();
        return services;
    }
}
