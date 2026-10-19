using MediatR;
using Microsoft.EntityFrameworkCore;
using PharmacyMS.Application.Common;
using PharmacyMS.Application.DTOs;
using PharmacyMS.Application.Interfaces;

namespace PharmacyMS.Application.Features.Generics.Queries;

public sealed record GetGenericsQuery() : IRequest<IReadOnlyList<GenericDto>>;
public sealed class GetGenericsHandler(IApplicationDbContext db) : IRequestHandler<GetGenericsQuery,IReadOnlyList<GenericDto>>
{
    public async Task<IReadOnlyList<GenericDto>> Handle(GetGenericsQuery request,CancellationToken ct) =>
        await db.Generics.AsNoTracking().OrderBy(x => x.Id)
            .Select(x => new GenericDto(x.Id, x.Code, x.Name)).ToListAsync(ct);
}

public sealed record GetGenericQuery(int Id) : IRequest<GenericDto>;
public sealed class GetGenericHandler(IApplicationDbContext db) : IRequestHandler<GetGenericQuery,GenericDto>
{
    public async Task<GenericDto> Handle(GetGenericQuery request,CancellationToken ct) =>
        await db.Generics.AsNoTracking().Where(x => x.Id == request.Id)
            .Select(x => new GenericDto(x.Id, x.Code, x.Name)).SingleOrDefaultAsync(ct)
            ?? throw new ApiException(404,"Generic not found.");
}
