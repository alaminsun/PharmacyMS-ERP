using FluentValidation;
using MediatR;
using PharmacyMS.Application.DTOs;
using PharmacyMS.Application.Interfaces;

namespace PharmacyMS.Application.Features.Auth.Commands;

public sealed record LoginCommand(LoginRequest Input) : IRequest<LoginResponse?>;
public sealed class LoginHandler(IIdentityService service) : IRequestHandler<LoginCommand,LoginResponse?>
{
    public Task<LoginResponse?> Handle(LoginCommand request,CancellationToken ct) => service.LoginAsync(request.Input,ct);
}
public sealed record CreateUserCommand(CreateUserRequest Input) : IRequest<string>;
public sealed class CreateUserHandler(IIdentityService service) : IRequestHandler<CreateUserCommand,string>
{
    public Task<string> Handle(CreateUserCommand request,CancellationToken ct) => service.CreateUserAsync(request.Input,ct);
}
public sealed record AssignRoleCommand(AssignRoleRequest Input) : IRequest;
public sealed class AssignRoleHandler(IIdentityService service) : IRequestHandler<AssignRoleCommand>
{
    public Task Handle(AssignRoleCommand request,CancellationToken ct) => service.AssignRoleAsync(request.Input,ct);
}
public sealed record CreateRoleCommand(CreateRoleRequest Input) : IRequest;
public sealed class CreateRoleHandler(IIdentityService service) : IRequestHandler<CreateRoleCommand>
{
    public Task Handle(CreateRoleCommand request,CancellationToken ct) => service.CreateRoleAsync(request.Input,ct);
}
public sealed record GrantPermissionCommand(GrantPermissionRequest Input) : IRequest;
public sealed class GrantPermissionHandler(IIdentityService service) : IRequestHandler<GrantPermissionCommand>
{
    public Task Handle(GrantPermissionCommand request,CancellationToken ct) => service.GrantPermissionAsync(request.Input,ct);
}
public sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x=>x.Input.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x=>x.Input.Password).NotEmpty();
    }
}
public sealed class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x=>x.Input.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x=>x.Input.Password).MinimumLength(12).MaximumLength(128);
        RuleFor(x=>x.Input.Role).NotEmpty().MaximumLength(100);
    }
}

public sealed record RevokePermissionCommand(RevokePermissionRequest Input) : IRequest;
public sealed class RevokePermissionHandler(IIdentityService service) : IRequestHandler<RevokePermissionCommand>
{
    public Task Handle(RevokePermissionCommand request,CancellationToken ct) => service.RevokePermissionAsync(request.Input,ct);
}
public sealed record RemoveRoleCommand(RemoveRoleRequest Input) : IRequest;
public sealed class RemoveRoleHandler(IIdentityService service) : IRequestHandler<RemoveRoleCommand>
{
    public Task Handle(RemoveRoleCommand request,CancellationToken ct) => service.RemoveRoleAsync(request.Input,ct);
}
public sealed record SetUserStatusCommand(SetUserStatusRequest Input) : IRequest;
public sealed class SetUserStatusHandler(IIdentityService service) : IRequestHandler<SetUserStatusCommand>
{
    public Task Handle(SetUserStatusCommand request,CancellationToken ct) => service.SetUserStatusAsync(request.Input,ct);
}
public sealed record SeedDefaultRolesCommand() : IRequest<IReadOnlyList<string>>;
public sealed class SeedDefaultRolesHandler(IIdentityService service) : IRequestHandler<SeedDefaultRolesCommand,IReadOnlyList<string>>
{
    public Task<IReadOnlyList<string>> Handle(SeedDefaultRolesCommand request,CancellationToken ct) => service.SeedDefaultRolesAsync(ct);
}
