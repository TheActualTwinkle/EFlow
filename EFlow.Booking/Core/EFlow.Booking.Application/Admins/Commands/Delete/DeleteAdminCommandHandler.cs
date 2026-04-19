using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Application.Common.Errors.Identity;
using EFlow.Booking.Domain;
using EFlow.Booking.Domain.Admins;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EFlow.Booking.Application.Admins.Commands;

public class DeleteAdminCommandHandler(IUnitOfWork unitOfWork, UserManager<Identity> userManager)
    : IRequestHandler<DeleteAdminCommand, Result>
{
    public async Task<Result> Handle(DeleteAdminCommand request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.GetRepository<IAdminRepository>();
        var identity = await userManager.FindByIdAsync(request.Id.ToString());

        if (identity is null)
            return Result.Ok();

        var result = await userManager.DeleteAsync(identity);

        if (!result.Succeeded)
            return Result.Fail(
                new IdentityInternalError()
                    .WithMessage("Failed to delete admin")
                    .WithIdentityErrors(result.Errors));

        var admin = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (admin is null)
            return Result.Ok();

        admin.Delete();

        await repository.DeleteAsync(admin);

        return Result.Ok();
    }
}