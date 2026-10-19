using PharmacyMS.Application.DTOs;
namespace PharmacyMS.Application.Interfaces;

public interface IIdentityService
{
    Task<LoginResponse?> LoginAsync(LoginRequest input,CancellationToken ct);
    Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken ct);
    Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken ct);
    Task<string> CreateUserAsync(CreateUserRequest input,CancellationToken ct);
    Task AssignRoleAsync(AssignRoleRequest input,CancellationToken ct);
    Task CreateRoleAsync(CreateRoleRequest input,CancellationToken ct);
    Task GrantPermissionAsync(GrantPermissionRequest input,CancellationToken ct);
    Task RevokePermissionAsync(RevokePermissionRequest input,CancellationToken ct);
    Task RemoveRoleAsync(RemoveRoleRequest input,CancellationToken ct);
    Task SetUserStatusAsync(SetUserStatusRequest input,CancellationToken ct);
    Task<IReadOnlyList<string>> SeedDefaultRolesAsync(CancellationToken ct);
    Task<MyAccessDto> GetMyAccessAsync(string userId,CancellationToken ct);
}
