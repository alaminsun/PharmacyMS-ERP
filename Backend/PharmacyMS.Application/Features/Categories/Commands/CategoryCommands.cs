using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PharmacyMS.Application.Common;
using PharmacyMS.Application.DTOs;
using PharmacyMS.Application.Interfaces;
using PharmacyMS.Domain.Entities;

namespace PharmacyMS.Application.Features.Categories.Commands;

public sealed record CreateCategoryCommand(CategoryInput Input) : IRequest<int>;
public sealed class CreateCategoryHandler(IApplicationDbContext db) : IRequestHandler<CreateCategoryCommand,int>
{
    public async Task<int> Handle(CreateCategoryCommand request,CancellationToken ct)
    {
        if (await db.Categories.AnyAsync(x=>x.Name == request.Input.Name,ct))
            throw new ApiException(409,"Category name already exists.");
        var entity = new Category {
            Name = request.Input.Name,
            Description = request.Input.Description,
        };
        db.Categories.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity.Id;
    }
}

public sealed record UpdateCategoryCommand(int Id, CategoryInput Input) : IRequest;
public sealed class UpdateCategoryHandler(IApplicationDbContext db) : IRequestHandler<UpdateCategoryCommand>
{
    public async Task Handle(UpdateCategoryCommand request,CancellationToken ct)
    {
        var entity = await db.Categories.FindAsync(new object[]{request.Id},ct)
            ?? throw new ApiException(404,"Category not found.");
        if (await db.Categories.AnyAsync(x=>x.Id != request.Id && x.Name == request.Input.Name,ct))
            throw new ApiException(409,"Category name already exists.");
        entity.Name = request.Input.Name;
        entity.Description = request.Input.Description;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}

public sealed record DeleteCategoryCommand(int Id) : IRequest;
public sealed class DeleteCategoryHandler(IApplicationDbContext db) : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand request,CancellationToken ct)
    {
        var entity = await db.Categories.FindAsync(new object[]{request.Id},ct)
            ?? throw new ApiException(404,"Category not found.");
        db.Categories.Remove(entity);
        await db.SaveChangesAsync(ct);
    }
}

public sealed class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator() => RuleFor(x=>x.Input).SetValidator(new CategoryInputValidator());
}
public sealed class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x=>x.Id).GreaterThan(0);
        RuleFor(x=>x.Input).SetValidator(new CategoryInputValidator());
    }
}
public sealed class CategoryInputValidator : AbstractValidator<CategoryInput>
{
    public CategoryInputValidator()
    {
        RuleFor(x=>x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x=>x.Description).MaximumLength(500);
    }
}
