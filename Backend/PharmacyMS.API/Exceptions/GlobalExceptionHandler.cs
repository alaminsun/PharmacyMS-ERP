using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using PharmacyMS.Application.Common;

namespace PharmacyMS.API.Exceptions;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context,Exception exception,CancellationToken ct)
    {
        logger.LogError(exception,"API request failed at {Path}",context.Request.Path);
        var status=exception switch
        {
            ApiException e => e.StatusCode,
            ValidationException => 400,
            DbUpdateConcurrencyException => 409,
            DbUpdateException e when e.InnerException is SqlException s && (s.Number==2601 || s.Number==2627 || s.Number==547) => 409,
            DbUpdateException e when e.InnerException is SqlException s && s.Number==1205 => 409,
            SqlException s when s.Number==1205 => 409,
            SqlException => 503,
            DbUpdateException e when e.InnerException is SqlException => 503,
            _ => 500
        };
        var title=status switch { 400=>"Invalid request",401=>"Unauthorized",404=>"Not found",409=>"Conflict",503=>"Database unavailable",_=>"Request failed"};
        var detail=exception is ApiException or ValidationException ? exception.Message :
            status==409 ? "Data changed or conflicted with existing records; refresh and retry." :
            status==503 ? "Database is unavailable; check API logs." : "An unexpected error occurred.";
        context.Response.StatusCode=status;
        var details=new ProblemDetails {Status=status,Title=title,Detail=detail,Instance=context.Request.Path};
        if (exception is ValidationException vex)
            details.Extensions["errors"]=vex.Errors.GroupBy(x=>x.PropertyName).ToDictionary(x=>x.Key,x=>x.Select(e=>e.ErrorMessage).ToArray());
        await context.Response.WriteAsJsonAsync(details,ct);
        return true;
    }
}
