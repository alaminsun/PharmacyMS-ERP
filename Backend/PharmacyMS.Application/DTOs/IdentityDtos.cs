namespace PharmacyMS.Application.DTOs;

public sealed record LoginRequest(string Email,string Password);
public sealed record LoginResponse(string AccessToken, DateTime ExpiresAtUtc, string Email, IReadOnlyList<string> Roles);
public sealed record CreateUserRequest(string Email,string Password,string Role);
public sealed record AssignRoleRequest(string Email,string Role);
public sealed record GrantPermissionRequest(string Role,string Permission);
public sealed record RevokePermissionRequest(string Role,string Permission);
public sealed record RemoveRoleRequest(string Email,string Role);
public sealed record SetUserStatusRequest(string Email,bool IsActive);
public sealed record MyAccessDto(string UserId,string Email,IReadOnlyList<string> Roles,IReadOnlyList<string> Permissions);
public sealed record CreateRoleRequest(string Name);
public sealed record UserDto(string Id,string Email,bool IsActive,IReadOnlyList<string> Roles);
public sealed record RoleDto(string Name,IReadOnlyList<string> Permissions);
