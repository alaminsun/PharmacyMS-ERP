using MediatR;
using Microsoft.EntityFrameworkCore;
using PharmacyMS.Application.Common;
using PharmacyMS.Application.DTOs;
using PharmacyMS.Application.Interfaces;

namespace PharmacyMS.Application.Features.Customers.Queries;

public sealed record GetCustomersQuery() : IRequest<IReadOnlyList<CustomerDto>>;
public sealed class GetCustomersHandler(IApplicationDbContext db) : IRequestHandler<GetCustomersQuery,IReadOnlyList<CustomerDto>>
{
    public async Task<IReadOnlyList<CustomerDto>> Handle(GetCustomersQuery request,CancellationToken ct) =>
        await db.Customers.AsNoTracking().OrderBy(x => x.Id)
            .Select(x => new CustomerDto(x.Id, x.Code, x.Name, x.Email, x.Phone, x.Address, x.Notes)).ToListAsync(ct);
}

public sealed record GetCustomerQuery(int Id) : IRequest<CustomerDto>;
public sealed class GetCustomerHandler(IApplicationDbContext db) : IRequestHandler<GetCustomerQuery,CustomerDto>
{
    public async Task<CustomerDto> Handle(GetCustomerQuery request,CancellationToken ct) =>
        await db.Customers.AsNoTracking().Where(x => x.Id == request.Id)
            .Select(x => new CustomerDto(x.Id, x.Code, x.Name, x.Email, x.Phone, x.Address, x.Notes)).SingleOrDefaultAsync(ct)
            ?? throw new ApiException(404,"Customer not found.");
}
