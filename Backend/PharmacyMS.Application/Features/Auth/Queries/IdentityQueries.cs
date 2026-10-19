using MediatR;
using PharmacyMS.Application.DTOs;
using PharmacyMS.Application.Interfaces;
namespace PharmacyMS.Application.Features.Auth.Queries;

public sealed record GetUsersQuery() : IRequest<IReadOnlyList<UserDto>>;
public sealed class GetUsersHandler(IIdentityService service) : IRequestHandler<GetUsersQuery,IReadOnlyList<UserDto>>
{
    public Task<IReadOnlyList<UserDto>> Handle(GetUsersQuery request,CancellationToken ct)=>service.GetUsersAsync(ct);
}
public sealed record GetRolesQuery() : IRequest<IReadOnlyList<RoleDto>>;
public sealed class GetRolesHandler(IIdentityService service) : IRequestHandler<GetRolesQuery,IReadOnlyList<RoleDto>>
{
    public Task<IReadOnlyList<RoleDto>> Handle(GetRolesQuery request,CancellationToken ct)=>service.GetRolesAsync(ct);
}

public sealed record GetMyAccessQuery(string UserId) : IRequest<MyAccessDto>;
public sealed class GetMyAccessHandler(IIdentityService service) : IRequestHandler<GetMyAccessQuery,MyAccessDto>
{
    public Task<MyAccessDto> Handle(GetMyAccessQuery request,CancellationToken ct) => service.GetMyAccessAsync(request.UserId,ct);
}
