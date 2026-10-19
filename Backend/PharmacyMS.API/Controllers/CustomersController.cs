using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyMS.Application.DTOs;
using PharmacyMS.Application.Features.Customers.Commands;
using PharmacyMS.Application.Features.Customers.Queries;
using PharmacyMS.API.Security;

namespace PharmacyMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class CustomersController(ISender sender) : ControllerBase
{
    [HttpGet, HasPermission(Permissions.CustomersRead)]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(await sender.Send(new GetCustomersQuery(),ct));

    [HttpGet("{id:int}"), HasPermission(Permissions.CustomersRead)]
    public async Task<IActionResult> Get(int id,CancellationToken ct) => Ok(await sender.Send(new GetCustomerQuery(id),ct));

    [HttpPost, HasPermission(Permissions.CustomersWrite)]
    public async Task<IActionResult> Create([FromBody]CustomerInput input,CancellationToken ct)
    {
        var id = await sender.Send(new CreateCustomerCommand(input),ct);
        return CreatedAtAction(nameof(Get),new {id},new {id});
    }

    [HttpPut("{id:int}"), HasPermission(Permissions.CustomersWrite)]
    public async Task<IActionResult> Update(int id,[FromBody]CustomerInput input,CancellationToken ct)
    {
        await sender.Send(new UpdateCustomerCommand(id,input),ct);
        return NoContent();
    }

    [HttpDelete("{id:int}"), HasPermission(Permissions.CustomersWrite)]
    public async Task<IActionResult> Delete(int id,CancellationToken ct)
    {
        await sender.Send(new DeleteCustomerCommand(id),ct);
        return NoContent();
    }
}
