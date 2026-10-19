using MediatR;
using Microsoft.EntityFrameworkCore;
using PharmacyMS.Application.Common;
using PharmacyMS.Application.DTOs;
using PharmacyMS.Application.Interfaces;

namespace PharmacyMS.Application.Features.Medicines.Queries;

public sealed record GetMedicinesQuery(string? Search = null) : IRequest<IReadOnlyList<MedicineDto>>;
public sealed class GetMedicinesHandler(IApplicationDbContext db) : IRequestHandler<GetMedicinesQuery,IReadOnlyList<MedicineDto>>
{
    public async Task<IReadOnlyList<MedicineDto>> Handle(GetMedicinesQuery request,CancellationToken ct)
    {
        var q = db.Medicines.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            q = q.Where(x=>x.Code.Contains(term) || x.Name.Contains(term));
        }
        return await q.OrderBy(x=>x.Name)
            .Select(x => new MedicineDto(x.Id,x.Code,x.Name,x.Strength,
                x.CategoryId,x.Category != null ? x.Category.Name : null,
                x.GenericId,x.Generic != null ? x.Generic.Name : null,
                x.ShelfId,x.Shelf != null ? x.Shelf.Name : null,
                x.SupplierId,x.Supplier != null ? x.Supplier.Name : null,
                x.BuyingPrice,x.SellingPrice,x.IsActive,x.MinimumStockLevel)).ToListAsync(ct);
    }
}
public sealed record GetMedicineQuery(int Id) : IRequest<MedicineDto>;
public sealed class GetMedicineHandler(IApplicationDbContext db) : IRequestHandler<GetMedicineQuery,MedicineDto>
{
    public async Task<MedicineDto> Handle(GetMedicineQuery request,CancellationToken ct) =>
        await db.Medicines.AsNoTracking().Where(x=>x.Id==request.Id)
            .Select(x=>new MedicineDto(x.Id,x.Code,x.Name,x.Strength,
                x.CategoryId,x.Category != null ? x.Category.Name : null,
                x.GenericId,x.Generic != null ? x.Generic.Name : null,
                x.ShelfId,x.Shelf != null ? x.Shelf.Name : null,
                x.SupplierId,x.Supplier != null ? x.Supplier.Name : null,
                x.BuyingPrice,x.SellingPrice,x.IsActive,x.MinimumStockLevel)).SingleOrDefaultAsync(ct)
            ?? throw new ApiException(404,"Medicine not found.");
}
