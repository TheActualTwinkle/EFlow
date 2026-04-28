using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Application.Common.Errors.Identity;
using EFlow.Booking.Domain;
using EFlow.Booking.Domain.Admins;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EFlow.Booking.Application.Admins.Commands;

public class CreateAdminCommandHandler(
    IUnitOfWork unitOfWork,
    UserManager<Identity> userManager,
    ISystemClock systemClock)
    : IRequestHandler<CreateAdminCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateAdminCommand request, CancellationToken cancellationToken)
    {
        var identity = new Identity
        {
            Id = Guid.NewGuid(),
            UserName = request.UserName,
            Email = request.Email
        };

        var createUserResult = await userManager.CreateAsync(identity, request.Password);

        if (!createUserResult.Succeeded)
            return Result.Fail(
                new IdentityInternalError()
                    .WithMessage("Failed to create user")
                    .WithIdentityErrors(createUserResult.Errors));

        var addToRoleResult = await userManager.AddToRoleAsync(identity, Identity.Roles.Admin);

        if (!addToRoleResult.Succeeded)
            return Result.Fail(
                new IdentityInternalError()
                    .WithMessage("Failed to add user to role")
                    .WithIdentityErrors(addToRoleResult.Errors));

        var nowUtc = systemClock.UtcNow;

        var admin = Admin.Create(new AdminId(identity.Id), nowUtc, nowUtc);

        await unitOfWork
            .GetRepository<IAdminRepository>()
            .CreateAsync(admin, cancellationToken);

        return Result.Ok(admin.Id.Value);
    }
}
