using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyMS.API.Security;
using PharmacyMS.Application.DTOs;
using PharmacyMS.Application.Features.Purchases.Commands;
using PharmacyMS.Application.Features.Purchases.Queries;

namespace PharmacyMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class PurchasesController(ISender sender) : ControllerBase
{
    [HttpGet, HasPermission(Permissions.PurchasesRead)]
    public async Task<IActionResult> Get(CancellationToken ct) => Ok(await sender.Send(new GetPurchasesQuery(),ct));

    [HttpPost, HasPermission(Permissions.PurchasesWrite)]
    public async Task<IActionResult> Create([FromBody]CreatePurchaseRequest input,CancellationToken ct)
    {
        var id=await sender.Send(new CreatePurchaseCommand(input),ct);
        return Ok(new {id});
    }
}
