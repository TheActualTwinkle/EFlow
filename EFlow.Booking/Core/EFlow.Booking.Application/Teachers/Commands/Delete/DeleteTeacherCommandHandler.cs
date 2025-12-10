using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Application.Common.Errors.Identity;
using EFlow.Booking.Domain;
using EFlow.Booking.Domain.Models;
using EFlow.Booking.Domain.Repositories;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EFlow.Booking.Application.Teachers.Commands;

public class DeleteTeacherCommandHandler(IUnitOfWork unitOfWork, UserManager<Identity> userManager) : IRequestHandler<DeleteTeacherCommand, Result>
{
    public async Task<Result> Handle(DeleteTeacherCommand request, CancellationToken cancellationToken)
    {
        var identity = await userManager.FindByIdAsync(request.Id.ToString());

        if (identity is null)
            return Result.Ok();

        var result = await userManager.DeleteAsync(identity);

        if (!result.Succeeded)
            return Result.Fail(
                new IdentityInternalError()
                    .WithMessage("Failed to delete user")
                    .WithIdentityErrors(result.Errors));

        await unitOfWork
            .GetRepository<ITeacherRepository>()
            .DeleteAsync(request.Id, cancellationToken);

        return Result.Ok();
    }
}