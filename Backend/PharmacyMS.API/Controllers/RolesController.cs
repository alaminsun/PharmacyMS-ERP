using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyMS.API.Security;
using PharmacyMS.Application.DTOs;
using PharmacyMS.Application.Features.Auth.Commands;
using PharmacyMS.Application.Features.Auth.Queries;
using PharmacyMS.Infrastructure.Security;

namespace PharmacyMS.API.Controllers;

[ApiController]
[Route("api/admin/roles")]
[Authorize]
public sealed class RolesController(ISender sender) : ControllerBase
{
    [HttpGet,HasPermission(Permissions.RolesManage)]
    public async Task<IActionResult> List(CancellationToken ct)=>Ok(await sender.Send(new GetRolesQuery(),ct));

    [HttpGet("permissions"),HasPermission(Permissions.RolesManage)]
    public IActionResult PermissionsList()=>Ok(PermissionNames.All);

    [HttpPost,HasPermission(Permissions.RolesManage)]
    public async Task<IActionResult> Create([FromBody]CreateRoleRequest input,CancellationToken ct)
    {
        await sender.Send(new CreateRoleCommand(input),ct);
        return Ok(new{created=true});
    }

    [HttpPost("grant-permission"),HasPermission(Permissions.RolesManage)]
    public async Task<IActionResult> Grant([FromBody]GrantPermissionRequest input,CancellationToken ct)
    {
        await sender.Send(new GrantPermissionCommand(input),ct);
        return NoContent();
    }

    [HttpPost("revoke-permission"),HasPermission(Permissions.RolesManage)]
    public async Task<IActionResult> Revoke([FromBody]RevokePermissionRequest input,CancellationToken ct)
    {
        await sender.Send(new RevokePermissionCommand(input),ct);
        return NoContent();
    }

    [HttpPost("seed-defaults"),HasPermission(Permissions.RolesManage)]
    public async Task<IActionResult> SeedDefaults(CancellationToken ct)
    {
        var created=await sender.Send(new SeedDefaultRolesCommand(),ct);
        return Ok(new { createdRoles=created });
    }
}
