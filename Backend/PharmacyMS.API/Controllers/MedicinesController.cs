using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyMS.API.Security;
using PharmacyMS.Application.DTOs;
using PharmacyMS.Application.Features.Medicines.Commands;
using PharmacyMS.Application.Features.Medicines.Queries;

namespace PharmacyMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class MedicinesController(ISender sender) : ControllerBase
{
    [HttpGet,HasPermission(Permissions.MedicinesRead)]
    public async Task<IActionResult> List([FromQuery]string? search,CancellationToken ct) => Ok(await sender.Send(new GetMedicinesQuery(search),ct));

    [HttpGet("{id:int}"),HasPermission(Permissions.MedicinesRead)]
    public async Task<IActionResult> Get(int id,CancellationToken ct) => Ok(await sender.Send(new GetMedicineQuery(id),ct));

    [HttpPost,HasPermission(Permissions.MedicinesWrite)]
    public async Task<IActionResult> Create([FromBody]MedicineInput input,CancellationToken ct)
    {
        var id=await sender.Send(new CreateMedicineCommand(input),ct);
        return CreatedAtAction(nameof(Get),new {id},new {id});
    }

    [HttpPut("{id:int}"),HasPermission(Permissions.MedicinesWrite)]
    public async Task<IActionResult> Update(int id,[FromBody]MedicineInput input,CancellationToken ct)
    {
        await sender.Send(new UpdateMedicineCommand(id,input),ct);
        return NoContent();
    }

    [HttpDelete("{id:int}"),HasPermission(Permissions.MedicinesWrite)]
    public async Task<IActionResult> Deactivate(int id,CancellationToken ct)
    {
        await sender.Send(new DeleteMedicineCommand(id),ct);
        return NoContent();
    }
}
