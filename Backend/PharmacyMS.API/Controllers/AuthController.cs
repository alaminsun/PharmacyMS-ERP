using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyMS.Application.DTOs;
using PharmacyMS.Application.Features.Auth.Commands;
using PharmacyMS.Application.Features.Auth.Queries;
using System.Security.Claims;

namespace PharmacyMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(ISender sender) : ControllerBase
{
    [HttpPost("login"),AllowAnonymous]
    public async Task<IActionResult> Login([FromBody]LoginRequest input,CancellationToken ct)
    {
        var result=await sender.Send(new LoginCommand(input),ct);
        return result is null ? Unauthorized(new {message="Invalid email or password."}) : Ok(result);
    }
    [HttpGet("me"),Authorize]
    public IActionResult Me()=>Ok(new {
        userId=User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
        email=User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
    });

    // Read effective DB permissions; JWT role claims may be stale after role changes.
    [HttpGet("me/permissions"),Authorize]
    public async Task<IActionResult> MyPermissions(CancellationToken ct)
    {
        var userId=User.FindFirstValue(ClaimTypes.NameIdentifier);
        return string.IsNullOrWhiteSpace(userId)
            ? Unauthorized()
            : Ok(await sender.Send(new GetMyAccessQuery(userId),ct));
    }
}
