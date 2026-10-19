using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PharmacyMS.Application.Common;
using PharmacyMS.Application.DTOs;
using PharmacyMS.Application.Interfaces;
using PharmacyMS.Domain.Entities;

namespace PharmacyMS.Application.Features.Medicines.Commands;

public sealed record CreateMedicineCommand(MedicineInput Input) : IRequest<int>;
public sealed class CreateMedicineHandler(IApplicationDbContext db) : IRequestHandler<CreateMedicineCommand,int>
{
    public async Task<int> Handle(CreateMedicineCommand request,CancellationToken ct)
    {
        var v = request.Input;
        if (await db.Medicines.AnyAsync(m => m.Code == v.Code,ct)) throw new ApiException(409,"Medicine code already exists.");
        var medicine = new Medicine {Code=v.Code,Name=v.Name,Strength=v.Strength,
            CategoryId=v.CategoryId,GenericId=v.GenericId,ShelfId=v.ShelfId,SupplierId=v.SupplierId,
            BuyingPrice=v.BuyingPrice,SellingPrice=v.SellingPrice,IsActive=v.IsActive,
            MinimumStockLevel=v.MinimumStockLevel};
        db.Medicines.Add(medicine);
        await db.SaveChangesAsync(ct);
        return medicine.Id;
    }
}
public sealed record UpdateMedicineCommand(int Id,MedicineInput Input) : IRequest;
public sealed class UpdateMedicineHandler(IApplicationDbContext db) : IRequestHandler<UpdateMedicineCommand>
{
    public async Task Handle(UpdateMedicineCommand request,CancellationToken ct)
    {
        var entity = await db.Medicines.FindAsync(new object[]{request.Id},ct)
            ?? throw new ApiException(404,"Medicine not found.");
        var v = request.Input;
        if (!string.Equals(entity.Code,v.Code,StringComparison.Ordinal))
            throw new ApiException(400,"Existing medicine code cannot be changed.");
        entity.Name=v.Name; entity.Strength=v.Strength;
        entity.CategoryId=v.CategoryId;entity.GenericId=v.GenericId;
        entity.ShelfId=v.ShelfId;entity.SupplierId=v.SupplierId;
        entity.BuyingPrice=v.BuyingPrice;entity.SellingPrice=v.SellingPrice;
        entity.MinimumStockLevel=v.MinimumStockLevel;
        entity.IsActive=v.IsActive;entity.UpdatedAtUtc=DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
public sealed record DeleteMedicineCommand(int Id) : IRequest;
public sealed class DeleteMedicineHandler(IApplicationDbContext db) : IRequestHandler<DeleteMedicineCommand>
{
    public async Task Handle(DeleteMedicineCommand request,CancellationToken ct)
    {
        var entity = await db.Medicines.FindAsync(new object[]{request.Id},ct)
            ?? throw new ApiException(404,"Medicine not found.");
        // Preserve stock and financial history; deactivate instead of deleting.
        entity.IsActive=false;entity.UpdatedAtUtc=DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
public sealed class CreateMedicineValidator : AbstractValidator<CreateMedicineCommand>
{
    public CreateMedicineValidator() => RuleFor(x=>x.Input).SetValidator(new MedicineInputValidator());
}
public sealed class UpdateMedicineValidator : AbstractValidator<UpdateMedicineCommand>
{
    public UpdateMedicineValidator()
    {
        RuleFor(x=>x.Id).GreaterThan(0);
        RuleFor(x=>x.Input).SetValidator(new MedicineInputValidator());
    }
}
public sealed class MedicineInputValidator : AbstractValidator<MedicineInput>
{
    public MedicineInputValidator()
    {
        RuleFor(x=>x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x=>x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x=>x.Strength).MaximumLength(100);
        RuleFor(x=>x.BuyingPrice).GreaterThanOrEqualTo(0).PrecisionScale(18,2,true);
        RuleFor(x=>x.SellingPrice).GreaterThanOrEqualTo(0).PrecisionScale(18,2,true);
        RuleFor(x=>x.MinimumStockLevel).InclusiveBetween(0,1000000);
        RuleFor(x=>x.CategoryId).GreaterThan(0).When(x=>x.CategoryId.HasValue);
        RuleFor(x=>x.GenericId).GreaterThan(0).When(x=>x.GenericId.HasValue);
        RuleFor(x=>x.ShelfId).GreaterThan(0).When(x=>x.ShelfId.HasValue);
        RuleFor(x=>x.SupplierId).GreaterThan(0).When(x=>x.SupplierId.HasValue);
    }
}
