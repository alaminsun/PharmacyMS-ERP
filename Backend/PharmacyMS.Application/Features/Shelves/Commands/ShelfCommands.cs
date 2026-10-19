using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PharmacyMS.Application.Common;
using PharmacyMS.Application.DTOs;
using PharmacyMS.Application.Interfaces;
using PharmacyMS.Domain.Entities;

namespace PharmacyMS.Application.Features.Shelves.Commands;

public sealed record CreateShelfCommand(ShelfInput Input) : IRequest<int>;
public sealed class CreateShelfHandler(IApplicationDbContext db) : IRequestHandler<CreateShelfCommand,int>
{
    public async Task<int> Handle(CreateShelfCommand request,CancellationToken ct)
    {
        if (await db.Shelves.AnyAsync(x=>x.Number == request.Input.Number,ct))
            throw new ApiException(409,"Shelf number already exists.");
        var entity = new Shelf {
            Name = request.Input.Name,
            Number = request.Input.Number,
        };
        db.Shelves.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity.Id;
    }
}

public sealed record UpdateShelfCommand(int Id, ShelfInput Input) : IRequest;
public sealed class UpdateShelfHandler(IApplicationDbContext db) : IRequestHandler<UpdateShelfCommand>
{
    public async Task Handle(UpdateShelfCommand request,CancellationToken ct)
    {
        var entity = await db.Shelves.FindAsync(new object[]{request.Id},ct)
            ?? throw new ApiException(404,"Shelf not found.");
        if (await db.Shelves.AnyAsync(x=>x.Id != request.Id && x.Number == request.Input.Number,ct))
            throw new ApiException(409,"Shelf number already exists.");
        entity.Name = request.Input.Name;
        entity.Number = request.Input.Number;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}

public sealed record DeleteShelfCommand(int Id) : IRequest;
public sealed class DeleteShelfHandler(IApplicationDbContext db) : IRequestHandler<DeleteShelfCommand>
{
    public async Task Handle(DeleteShelfCommand request,CancellationToken ct)
    {
        var entity = await db.Shelves.FindAsync(new object[]{request.Id},ct)
            ?? throw new ApiException(404,"Shelf not found.");
        db.Shelves.Remove(entity);
        await db.SaveChangesAsync(ct);
    }
}

public sealed class CreateShelfValidator : AbstractValidator<CreateShelfCommand>
{
    public CreateShelfValidator() => RuleFor(x=>x.Input).SetValidator(new ShelfInputValidator());
}
public sealed class UpdateShelfValidator : AbstractValidator<UpdateShelfCommand>
{
    public UpdateShelfValidator()
    {
        RuleFor(x=>x.Id).GreaterThan(0);
        RuleFor(x=>x.Input).SetValidator(new ShelfInputValidator());
    }
}
public sealed class ShelfInputValidator : AbstractValidator<ShelfInput>
{
    public ShelfInputValidator()
    {
        RuleFor(x=>x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x=>x.Number).NotEmpty().MaximumLength(50);
    }
}
