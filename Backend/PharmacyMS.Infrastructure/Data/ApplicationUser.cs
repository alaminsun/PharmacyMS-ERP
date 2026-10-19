using Microsoft.AspNetCore.Identity;
namespace PharmacyMS.Infrastructure.Data;
public sealed class ApplicationUser : IdentityUser
{
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
