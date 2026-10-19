using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyMS.Application.DTOs;
using PharmacyMS.Application.Features.Shelves.Commands;
using PharmacyMS.Application.Features.Shelves.Queries;
using PharmacyMS.API.Security;

namespace PharmacyMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ShelvesController(ISender sender) : ControllerBase
{
    [HttpGet, HasPermission(Permissions.ShelvesRead)]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(await sender.Send(new GetShelvesQuery(),ct));

    [HttpGet("{id:int}"), HasPermission(Permissions.ShelvesRead)]
    public async Task<IActionResult> Get(int id,CancellationToken ct) => Ok(await sender.Send(new GetShelfQuery(id),ct));

    [HttpPost, HasPermission(Permissions.ShelvesWrite)]
    public async Task<IActionResult> Create([FromBody]ShelfInput input,CancellationToken ct)
    {
        var id = await sender.Send(new CreateShelfCommand(input),ct);
        return CreatedAtAction(nameof(Get),new {id},new {id});
    }

    [HttpPut("{id:int}"), HasPermission(Permissions.ShelvesWrite)]
    public async Task<IActionResult> Update(int id,[FromBody]ShelfInput input,CancellationToken ct)
    {
        await sender.Send(new UpdateShelfCommand(id,input),ct);
        return NoContent();
    }

    [HttpDelete("{id:int}"), HasPermission(Permissions.ShelvesWrite)]
    public async Task<IActionResult> Delete(int id,CancellationToken ct)
    {
        await sender.Send(new DeleteShelfCommand(id),ct);
        return NoContent();
    }
}
