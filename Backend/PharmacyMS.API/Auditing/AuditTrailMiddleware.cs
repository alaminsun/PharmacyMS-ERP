using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using PharmacyMS.Infrastructure.Data;

namespace PharmacyMS.API.Auditing;

/// <summary>Logs successful state-changing API calls after authorization. Best-effort: not an atomic financial ledger.</summary>
public sealed class AuditTrailMiddleware(RequestDelegate next,ILogger<AuditTrailMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context,ApplicationDbContext db)
    {
        await next(context);
        var method=context.Request.Method;
        if(method is not ("POST" or "PUT" or "PATCH" or "DELETE"))return;
        if(!context.Request.Path.StartsWithSegments("/api"))return;
        if(context.Request.Path.StartsWithSegments("/api/auth"))return;
        if(context.Response.StatusCode<200||context.Response.StatusCode>=300)return;
        if(context.User.Identity?.IsAuthenticated!=true)return;
        try{
            db.Set<AuditEvent>().Add(new AuditEvent{
                ActorId=context.User.FindFirstValue(ClaimTypes.NameIdentifier)??"unknown",
                ActorEmail=context.User.FindFirstValue(ClaimTypes.Email)??"unknown",
                Action=method,
                Path=context.Request.Path.Value?[..Math.Min(context.Request.Path.Value.Length,240)]??"",
                StatusCode=context.Response.StatusCode,
                TraceId=context.TraceIdentifier[..Math.Min(context.TraceIdentifier.Length,100)]
            });
            await db.SaveChangesAsync(context.RequestAborted);
        }catch(Exception ex){logger.LogError(ex,"Audit persistence failed for {Method} {Path} {TraceId}",method,context.Request.Path,context.TraceIdentifier);}
    }
}
