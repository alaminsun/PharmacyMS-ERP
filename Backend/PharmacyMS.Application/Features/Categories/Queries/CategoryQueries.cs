using MediatR;
using Microsoft.EntityFrameworkCore;
using PharmacyMS.Application.Common;
using PharmacyMS.Application.DTOs;
using PharmacyMS.Application.Interfaces;

namespace PharmacyMS.Application.Features.Categories.Queries;

public sealed record GetCategoriesQuery() : IRequest<IReadOnlyList<CategoryDto>>;
public sealed class GetCategoriesHandler(IApplicationDbContext db) : IRequestHandler<GetCategoriesQuery,IReadOnlyList<CategoryDto>>
{
    public async Task<IReadOnlyList<CategoryDto>> Handle(GetCategoriesQuery request,CancellationToken ct) =>
        await db.Categories.AsNoTracking().OrderBy(x => x.Id)
            .Select(x => new CategoryDto(x.Id, x.Name, x.Description)).ToListAsync(ct);
}

public sealed record GetCategoryQuery(int Id) : IRequest<CategoryDto>;
public sealed class GetCategoryHandler(IApplicationDbContext db) : IRequestHandler<GetCategoryQuery,CategoryDto>
{
    public async Task<CategoryDto> Handle(GetCategoryQuery request,CancellationToken ct) =>
        await db.Categories.AsNoTracking().Where(x => x.Id == request.Id)
            .Select(x => new CategoryDto(x.Id, x.Name, x.Description)).SingleOrDefaultAsync(ct)
            ?? throw new ApiException(404,"Category not found.");
}
