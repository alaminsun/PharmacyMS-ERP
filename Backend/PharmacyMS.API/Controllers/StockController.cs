using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyMS.API.Security;
using PharmacyMS.Application.Features.Stock.Queries;

namespace PharmacyMS.API.Controllers;
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class StockController(ISender sender) : ControllerBase
{
    [HttpGet,HasPermission(Permissions.StockRead)]
    public async Task<IActionResult> Get([FromQuery]bool includeZero,CancellationToken ct) => Ok(await sender.Send(new GetStockQuery(includeZero),ct));

    [HttpGet("low-stock"), HasPermission(Permissions.StockRead)]
    public async Task<IActionResult> GetLowStock(CancellationToken ct)
        => Ok(await sender.Send(new GetLowStockMedicinesQuery(), ct));
}
