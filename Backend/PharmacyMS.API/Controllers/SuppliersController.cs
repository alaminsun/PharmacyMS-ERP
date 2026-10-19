using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyMS.Application.DTOs;
using PharmacyMS.Application.Features.Suppliers.Commands;
using PharmacyMS.Application.Features.Suppliers.Queries;
using PharmacyMS.API.Security;

namespace PharmacyMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class SuppliersController(ISender sender) : ControllerBase
{
    [HttpGet, HasPermission(Permissions.SuppliersRead)]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(await sender.Send(new GetSuppliersQuery(),ct));

    [HttpGet("{id:int}"), HasPermission(Permissions.SuppliersRead)]
    public async Task<IActionResult> Get(int id,CancellationToken ct) => Ok(await sender.Send(new GetSupplierQuery(id),ct));

    [HttpPost, HasPermission(Permissions.SuppliersWrite)]
    public async Task<IActionResult> Create([FromBody]SupplierInput input,CancellationToken ct)
    {
        var id = await sender.Send(new CreateSupplierCommand(input),ct);
        return CreatedAtAction(nameof(Get),new {id},new {id});
    }

    [HttpPut("{id:int}"), HasPermission(Permissions.SuppliersWrite)]
    public async Task<IActionResult> Update(int id,[FromBody]SupplierInput input,CancellationToken ct)
    {
        await sender.Send(new UpdateSupplierCommand(id,input),ct);
        return NoContent();
    }

    [HttpDelete("{id:int}"), HasPermission(Permissions.SuppliersWrite)]
    public async Task<IActionResult> Delete(int id,CancellationToken ct)
    {
        await sender.Send(new DeleteSupplierCommand(id),ct);
        return NoContent();
    }
}
