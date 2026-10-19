using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyMS.API.Security;
using PharmacyMS.Infrastructure.Data;

namespace PharmacyMS.API.Controllers;

[ApiController]
[Route("api/audit")]
[Authorize]
public sealed class AuditController(ApplicationDbContext db):ControllerBase
{
    // Existing Users.Manage permission avoids silently granting sensitive access to seeded roles.
    [HttpGet]
    [HasPermission(Permissions.UsersManage)]
    public async Task<IActionResult> List([FromQuery]int page=1,[FromQuery]int pageSize=25,
        [FromQuery]string? actorEmail=null,[FromQuery]string? resource=null,CancellationToken ct=default)
    {
        if(page<1||pageSize<1||pageSize>100)return BadRequest(new{message="Invalid page or pageSize."});
        var q=db.Set<AuditEvent>().AsNoTracking().AsQueryable();
        if(!string.IsNullOrWhiteSpace(actorEmail))q=q.Where(x=>x.ActorEmail.Contains(actorEmail.Trim()));
        if(!string.IsNullOrWhiteSpace(resource))q=q.Where(x=>x.Path.Contains(resource.Trim()));
        var total=await q.CountAsync(ct);
        var items=await q.OrderByDescending(x=>x.OccurredAtUtc).ThenByDescending(x=>x.Id)
            .Skip((page-1)*pageSize).Take(pageSize)
            .Select(x=>new{x.Id,x.OccurredAtUtc,x.ActorEmail,x.Action,x.Path,x.StatusCode,x.TraceId}).ToListAsync(ct);
        return Ok(new PageResult<object>(items.Cast<object>().ToList(),total,page,pageSize));
    }
}
