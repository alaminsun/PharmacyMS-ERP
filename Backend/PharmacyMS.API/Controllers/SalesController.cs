using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyMS.API.Security;
using PharmacyMS.Application.DTOs;
using PharmacyMS.Application.Features.Sales.Commands;
using PharmacyMS.Application.Features.Sales.Queries;

namespace PharmacyMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class SalesController(ISender sender) : ControllerBase
{
    [HttpGet, HasPermission(Permissions.SalesRead)]
    public async Task<IActionResult> Get(CancellationToken ct) => Ok(await sender.Send(new GetSalesQuery(),ct));

    [HttpPost, HasPermission(Permissions.SalesWrite)]
    public async Task<IActionResult> Create([FromBody]CreateSaleRequest input,CancellationToken ct)
    {
        var id=await sender.Send(new CreateSaleCommand(input),ct);
        return Ok(new {id});
    }
}
