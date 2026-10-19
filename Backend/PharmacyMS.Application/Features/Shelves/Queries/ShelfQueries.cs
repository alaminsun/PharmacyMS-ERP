using MediatR;
using Microsoft.EntityFrameworkCore;
using PharmacyMS.Application.Common;
using PharmacyMS.Application.DTOs;
using PharmacyMS.Application.Interfaces;

namespace PharmacyMS.Application.Features.Shelves.Queries;

public sealed record GetShelvesQuery() : IRequest<IReadOnlyList<ShelfDto>>;
public sealed class GetShelvesHandler(IApplicationDbContext db) : IRequestHandler<GetShelvesQuery,IReadOnlyList<ShelfDto>>
{
    public async Task<IReadOnlyList<ShelfDto>> Handle(GetShelvesQuery request,CancellationToken ct) =>
        await db.Shelves.AsNoTracking().OrderBy(x => x.Id)
            .Select(x => new ShelfDto(x.Id, x.Name, x.Number)).ToListAsync(ct);
}

public sealed record GetShelfQuery(int Id) : IRequest<ShelfDto>;
public sealed class GetShelfHandler(IApplicationDbContext db) : IRequestHandler<GetShelfQuery,ShelfDto>
{
    public async Task<ShelfDto> Handle(GetShelfQuery request,CancellationToken ct) =>
        await db.Shelves.AsNoTracking().Where(x => x.Id == request.Id)
            .Select(x => new ShelfDto(x.Id, x.Name, x.Number)).SingleOrDefaultAsync(ct)
            ?? throw new ApiException(404,"Shelf not found.");
}
