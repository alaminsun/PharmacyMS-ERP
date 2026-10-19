using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PharmacyMS.Application.Common;
using PharmacyMS.Application.DTOs;
using PharmacyMS.Application.Interfaces;
using PharmacyMS.Domain.Entities;

namespace PharmacyMS.Application.Features.Generics.Commands;

public sealed record CreateGenericCommand(GenericInput Input) : IRequest<int>;
public sealed class CreateGenericHandler(IApplicationDbContext db) : IRequestHandler<CreateGenericCommand,int>
{
    public async Task<int> Handle(CreateGenericCommand request,CancellationToken ct)
    {
        if (await db.Generics.AnyAsync(x=>x.Code == request.Input.Code,ct))
            throw new ApiException(409,"Generic code already exists.");
        var entity = new Generic {
            Code = request.Input.Code,
            Name = request.Input.Name,
        };
        db.Generics.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity.Id;
    }
}

public sealed record UpdateGenericCommand(int Id, GenericInput Input) : IRequest;
public sealed class UpdateGenericHandler(IApplicationDbContext db) : IRequestHandler<UpdateGenericCommand>
{
    public async Task Handle(UpdateGenericCommand request,CancellationToken ct)
    {
        var entity = await db.Generics.FindAsync(new object[]{request.Id},ct)
            ?? throw new ApiException(404,"Generic not found.");
        if (await db.Generics.AnyAsync(x=>x.Id != request.Id && x.Code == request.Input.Code,ct))
            throw new ApiException(409,"Generic code already exists.");
        entity.Code = request.Input.Code;
        entity.Name = request.Input.Name;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}

public sealed record DeleteGenericCommand(int Id) : IRequest;
public sealed class DeleteGenericHandler(IApplicationDbContext db) : IRequestHandler<DeleteGenericCommand>
{
    public async Task Handle(DeleteGenericCommand request,CancellationToken ct)
    {
        var entity = await db.Generics.FindAsync(new object[]{request.Id},ct)
            ?? throw new ApiException(404,"Generic not found.");
        db.Generics.Remove(entity);
        await db.SaveChangesAsync(ct);
    }
}

public sealed class CreateGenericValidator : AbstractValidator<CreateGenericCommand>
{
    public CreateGenericValidator() => RuleFor(x=>x.Input).SetValidator(new GenericInputValidator());
}
public sealed class UpdateGenericValidator : AbstractValidator<UpdateGenericCommand>
{
    public UpdateGenericValidator()
    {
        RuleFor(x=>x.Id).GreaterThan(0);
        RuleFor(x=>x.Input).SetValidator(new GenericInputValidator());
    }
}
public sealed class GenericInputValidator : AbstractValidator<GenericInput>
{
    public GenericInputValidator()
    {
        RuleFor(x=>x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x=>x.Name).NotEmpty().MaximumLength(150);
    }
}
