using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyMS.Application.DTOs;
using PharmacyMS.Application.Features.Categories.Commands;
using PharmacyMS.Application.Features.Categories.Queries;
using PharmacyMS.API.Security;

namespace PharmacyMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class CategoriesController(ISender sender) : ControllerBase
{
    [HttpGet, HasPermission(Permissions.CategoriesRead)]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(await sender.Send(new GetCategoriesQuery(),ct));

    [HttpGet("{id:int}"), HasPermission(Permissions.CategoriesRead)]
    public async Task<IActionResult> Get(int id,CancellationToken ct) => Ok(await sender.Send(new GetCategoryQuery(id),ct));

    [HttpPost, HasPermission(Permissions.CategoriesWrite)]
    public async Task<IActionResult> Create([FromBody]CategoryInput input,CancellationToken ct)
    {
        var id = await sender.Send(new CreateCategoryCommand(input),ct);
        return CreatedAtAction(nameof(Get),new {id},new {id});
    }

    [HttpPut("{id:int}"), HasPermission(Permissions.CategoriesWrite)]
    public async Task<IActionResult> Update(int id,[FromBody]CategoryInput input,CancellationToken ct)
    {
        await sender.Send(new UpdateCategoryCommand(id,input),ct);
        return NoContent();
    }

    [HttpDelete("{id:int}"), HasPermission(Permissions.CategoriesWrite)]
    public async Task<IActionResult> Delete(int id,CancellationToken ct)
    {
        await sender.Send(new DeleteCategoryCommand(id),ct);
        return NoContent();
    }
}
