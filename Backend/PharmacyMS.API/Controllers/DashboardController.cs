using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyMS.API.Security;
using PharmacyMS.Application.Features.Dashboard.Queries;

namespace PharmacyMS.API.Controllers;
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class DashboardController(ISender sender) : ControllerBase
{
    [HttpGet,HasPermission(Permissions.DashboardRead)]
    public async Task<IActionResult> Get(CancellationToken ct) => Ok(await sender.Send(new GetDashboardQuery(),ct));
}
