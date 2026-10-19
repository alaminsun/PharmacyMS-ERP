using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyMS.API.Security;
using PharmacyMS.Application.DTOs;
using PharmacyMS.Application.Features.Auth.Commands;
using PharmacyMS.Application.Features.Auth.Queries;

namespace PharmacyMS.API.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize]
public sealed class UsersController(ISender sender) : ControllerBase
{
    [HttpGet,HasPermission(Permissions.UsersManage)]
    public async Task<IActionResult> List(CancellationToken ct)=>Ok(await sender.Send(new GetUsersQuery(),ct));

    [HttpPost,HasPermission(Permissions.UsersManage)]
    public async Task<IActionResult> Create([FromBody]CreateUserRequest input,CancellationToken ct)
    {
        var id=await sender.Send(new CreateUserCommand(input),ct);
        return Created($"/api/admin/users/{id}",new{id});
    }

    [HttpPost("assign-role"),HasPermission(Permissions.UsersManage)]
    public async Task<IActionResult> AssignRole([FromBody]AssignRoleRequest input,CancellationToken ct)
    {
        await sender.Send(new AssignRoleCommand(input),ct);
        return NoContent();
    }

    [HttpPost("remove-role"),HasPermission(Permissions.UsersManage)]
    public async Task<IActionResult> RemoveRole([FromBody]RemoveRoleRequest input,CancellationToken ct)
    {
        await sender.Send(new RemoveRoleCommand(input),ct);
        return NoContent();
    }

    [HttpPut("status"),HasPermission(Permissions.UsersManage)]
    public async Task<IActionResult> SetStatus([FromBody]SetUserStatusRequest input,CancellationToken ct)
    {
        await sender.Send(new SetUserStatusCommand(input),ct);
        return NoContent();
    }
}
