using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyMS.Application.DTOs;
using PharmacyMS.Application.Features.Generics.Commands;
using PharmacyMS.Application.Features.Generics.Queries;
using PharmacyMS.API.Security;

namespace PharmacyMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class GenericsController(ISender sender) : ControllerBase
{
    [HttpGet, HasPermission(Permissions.GenericsRead)]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(await sender.Send(new GetGenericsQuery(),ct));

    [HttpGet("{id:int}"), HasPermission(Permissions.GenericsRead)]
    public async Task<IActionResult> Get(int id,CancellationToken ct) => Ok(await sender.Send(new GetGenericQuery(id),ct));

    [HttpPost, HasPermission(Permissions.GenericsWrite)]
    public async Task<IActionResult> Create([FromBody]GenericInput input,CancellationToken ct)
    {
        var id = await sender.Send(new CreateGenericCommand(input),ct);
        return CreatedAtAction(nameof(Get),new {id},new {id});
    }

    [HttpPut("{id:int}"), HasPermission(Permissions.GenericsWrite)]
    public async Task<IActionResult> Update(int id,[FromBody]GenericInput input,CancellationToken ct)
    {
        await sender.Send(new UpdateGenericCommand(id,input),ct);
        return NoContent();
    }

    [HttpDelete("{id:int}"), HasPermission(Permissions.GenericsWrite)]
    public async Task<IActionResult> Delete(int id,CancellationToken ct)
    {
        await sender.Send(new DeleteGenericCommand(id),ct);
        return NoContent();
    }
}
