using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyMS.API.Security;
using PharmacyMS.Application.Features.Reports.Queries;

namespace PharmacyMS.API.Controllers;
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ReportsController(ISender sender) : ControllerBase
{
    [HttpGet, HasPermission(Permissions.ReportsRead)]
    public async Task<IActionResult> Get([FromQuery] DateTime fromUtc, [FromQuery] DateTime toUtc, CancellationToken ct)
        => Ok(await sender.Send(new GetReportQuery(fromUtc, toUtc), ct));

    [HttpGet("sales-details"), HasPermission(Permissions.ReportsRead)]
    public async Task<IActionResult> GetSalesDetails([FromQuery] DateTime fromUtc, [FromQuery] DateTime toUtc, CancellationToken ct)
        => Ok(await sender.Send(new GetSalesReportDetailsQuery(fromUtc, toUtc), ct));
}
