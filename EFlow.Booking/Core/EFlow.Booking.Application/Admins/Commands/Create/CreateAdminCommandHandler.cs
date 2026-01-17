using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Application.Common.Errors.Identity;
using EFlow.Common.Domain.Models;
using EFlow.Common.Domain;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EFlow.Booking.Application.Admins.Commands;

public class CreateAdminCommandHandler(
    IUnitOfWork unitOfWork,
    UserManager<Identity> userManager)
    : IRequestHandler<CreateAdminCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateAdminCommand request, CancellationToken cancellationToken)
    {
        var identity = new Identity { UserName = request.UserName };
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

        var admin = new Admin
        {
            Id = identity.Id,
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork
            .GetRepository<IAdminRepository>()
            .CreateAsync(admin, cancellationToken);

        return Result.Ok(identity.Id);
    }
}