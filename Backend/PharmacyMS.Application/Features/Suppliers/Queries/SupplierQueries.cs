using MediatR;
using Microsoft.EntityFrameworkCore;
using PharmacyMS.Application.Common;
using PharmacyMS.Application.DTOs;
using PharmacyMS.Application.Interfaces;

namespace PharmacyMS.Application.Features.Suppliers.Queries;

public sealed record GetSuppliersQuery() : IRequest<IReadOnlyList<SupplierDto>>;
public sealed class GetSuppliersHandler(IApplicationDbContext db) : IRequestHandler<GetSuppliersQuery,IReadOnlyList<SupplierDto>>
{
    public async Task<IReadOnlyList<SupplierDto>> Handle(GetSuppliersQuery request,CancellationToken ct) =>
        await db.Suppliers.AsNoTracking().OrderBy(x => x.Id)
            .Select(x => new SupplierDto(x.Id, x.Code, x.Name, x.ShortName, x.Email, x.Phone, x.Address, x.Notes)).ToListAsync(ct);
}

public sealed record GetSupplierQuery(int Id) : IRequest<SupplierDto>;
public sealed class GetSupplierHandler(IApplicationDbContext db) : IRequestHandler<GetSupplierQuery,SupplierDto>
{
    public async Task<SupplierDto> Handle(GetSupplierQuery request,CancellationToken ct) =>
        await db.Suppliers.AsNoTracking().Where(x => x.Id == request.Id)
            .Select(x => new SupplierDto(x.Id, x.Code, x.Name, x.ShortName, x.Email, x.Phone, x.Address, x.Notes)).SingleOrDefaultAsync(ct)
            ?? throw new ApiException(404,"Supplier not found.");
}
