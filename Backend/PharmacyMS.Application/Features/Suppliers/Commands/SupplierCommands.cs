using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PharmacyMS.Application.Common;
using PharmacyMS.Application.DTOs;
using PharmacyMS.Application.Interfaces;
using PharmacyMS.Domain.Entities;

namespace PharmacyMS.Application.Features.Suppliers.Commands;

public sealed record CreateSupplierCommand(SupplierInput Input) : IRequest<int>;
public sealed class CreateSupplierHandler(IApplicationDbContext db) : IRequestHandler<CreateSupplierCommand,int>
{
    public async Task<int> Handle(CreateSupplierCommand request,CancellationToken ct)
    {
        if (await db.Suppliers.AnyAsync(x=>x.Code == request.Input.Code,ct))
            throw new ApiException(409,"Supplier code already exists.");
        var entity = new Supplier {
            Code = request.Input.Code,
            Name = request.Input.Name,
            ShortName = request.Input.ShortName,
            Email = request.Input.Email,
            Phone = request.Input.Phone,
            Address = request.Input.Address,
            Notes = request.Input.Notes,
        };
        db.Suppliers.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity.Id;
    }
}

public sealed record UpdateSupplierCommand(int Id, SupplierInput Input) : IRequest;
public sealed class UpdateSupplierHandler(IApplicationDbContext db) : IRequestHandler<UpdateSupplierCommand>
{
    public async Task Handle(UpdateSupplierCommand request,CancellationToken ct)
    {
        var entity = await db.Suppliers.FindAsync(new object[]{request.Id},ct)
            ?? throw new ApiException(404,"Supplier not found.");
        if (await db.Suppliers.AnyAsync(x=>x.Id != request.Id && x.Code == request.Input.Code,ct))
            throw new ApiException(409,"Supplier code already exists.");
        entity.Code = request.Input.Code;
        entity.Name = request.Input.Name;
        entity.ShortName = request.Input.ShortName;
        entity.Email = request.Input.Email;
        entity.Phone = request.Input.Phone;
        entity.Address = request.Input.Address;
        entity.Notes = request.Input.Notes;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}

public sealed record DeleteSupplierCommand(int Id) : IRequest;
public sealed class DeleteSupplierHandler(IApplicationDbContext db) : IRequestHandler<DeleteSupplierCommand>
{
    public async Task Handle(DeleteSupplierCommand request,CancellationToken ct)
    {
        var entity = await db.Suppliers.FindAsync(new object[]{request.Id},ct)
            ?? throw new ApiException(404,"Supplier not found.");
        db.Suppliers.Remove(entity);
        await db.SaveChangesAsync(ct);
    }
}

public sealed class CreateSupplierValidator : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierValidator() => RuleFor(x=>x.Input).SetValidator(new SupplierInputValidator());
}
public sealed class UpdateSupplierValidator : AbstractValidator<UpdateSupplierCommand>
{
    public UpdateSupplierValidator()
    {
        RuleFor(x=>x.Id).GreaterThan(0);
        RuleFor(x=>x.Input).SetValidator(new SupplierInputValidator());
    }
}
public sealed class SupplierInputValidator : AbstractValidator<SupplierInput>
{
    public SupplierInputValidator()
    {
        RuleFor(x=>x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x=>x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x=>x.ShortName).MaximumLength(50);
        RuleFor(x=>x.Email).MaximumLength(200);
        RuleFor(x=>x.Phone).MaximumLength(40);
        RuleFor(x=>x.Address).MaximumLength(300);
        RuleFor(x=>x.Notes).MaximumLength(500);
        RuleFor(x=>x.Email).EmailAddress().When(x=>!string.IsNullOrWhiteSpace(x.Email));
    }
}
