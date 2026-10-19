using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PharmacyMS.Application.Common;
using PharmacyMS.Application.DTOs;
using PharmacyMS.Application.Interfaces;
using PharmacyMS.Domain.Entities;

namespace PharmacyMS.Application.Features.Customers.Commands;

public sealed record CreateCustomerCommand(CustomerInput Input) : IRequest<int>;
public sealed class CreateCustomerHandler(IApplicationDbContext db) : IRequestHandler<CreateCustomerCommand,int>
{
    public async Task<int> Handle(CreateCustomerCommand request,CancellationToken ct)
    {
        if (await db.Customers.AnyAsync(x=>x.Code == request.Input.Code,ct))
            throw new ApiException(409,"Customer code already exists.");
        var entity = new Customer {
            Code = request.Input.Code,
            Name = request.Input.Name,
            Email = request.Input.Email,
            Phone = request.Input.Phone,
            Address = request.Input.Address,
            Notes = request.Input.Notes,
        };
        db.Customers.Add(entity);
        await db.SaveChangesAsync(ct);
        return entity.Id;
    }
}

public sealed record UpdateCustomerCommand(int Id, CustomerInput Input) : IRequest;
public sealed class UpdateCustomerHandler(IApplicationDbContext db) : IRequestHandler<UpdateCustomerCommand>
{
    public async Task Handle(UpdateCustomerCommand request,CancellationToken ct)
    {
        var entity = await db.Customers.FindAsync(new object[]{request.Id},ct)
            ?? throw new ApiException(404,"Customer not found.");
        if (await db.Customers.AnyAsync(x=>x.Id != request.Id && x.Code == request.Input.Code,ct))
            throw new ApiException(409,"Customer code already exists.");
        entity.Code = request.Input.Code;
        entity.Name = request.Input.Name;
        entity.Email = request.Input.Email;
        entity.Phone = request.Input.Phone;
        entity.Address = request.Input.Address;
        entity.Notes = request.Input.Notes;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}

public sealed record DeleteCustomerCommand(int Id) : IRequest;
public sealed class DeleteCustomerHandler(IApplicationDbContext db) : IRequestHandler<DeleteCustomerCommand>
{
    public async Task Handle(DeleteCustomerCommand request,CancellationToken ct)
    {
        var entity = await db.Customers.FindAsync(new object[]{request.Id},ct)
            ?? throw new ApiException(404,"Customer not found.");
        db.Customers.Remove(entity);
        await db.SaveChangesAsync(ct);
    }
}

public sealed class CreateCustomerValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerValidator() => RuleFor(x=>x.Input).SetValidator(new CustomerInputValidator());
}
public sealed class UpdateCustomerValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerValidator()
    {
        RuleFor(x=>x.Id).GreaterThan(0);
        RuleFor(x=>x.Input).SetValidator(new CustomerInputValidator());
    }
}
public sealed class CustomerInputValidator : AbstractValidator<CustomerInput>
{
    public CustomerInputValidator()
    {
        RuleFor(x=>x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x=>x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x=>x.Email).MaximumLength(200);
        RuleFor(x=>x.Phone).MaximumLength(40);
        RuleFor(x=>x.Address).MaximumLength(300);
        RuleFor(x=>x.Notes).MaximumLength(500);
        RuleFor(x=>x.Email).EmailAddress().When(x=>!string.IsNullOrWhiteSpace(x.Email));
    }
}
