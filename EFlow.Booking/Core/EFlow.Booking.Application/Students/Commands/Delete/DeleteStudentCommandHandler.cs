using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Application.Common.Errors.Identity;
using EFlow.Common.Domain.Models;
using EFlow.Common.Domain;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EFlow.Booking.Application.Students.Commands;

public class DeleteStudentCommandHandler(IUnitOfWork unitOfWork, UserManager<Identity> userManager)
    : IRequestHandler<DeleteStudentCommand, Result>
{
    public async Task<Result> Handle(DeleteStudentCommand request, CancellationToken cancellationToken)
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
            .GetRepository<IStudentRepository>()
            .DeleteAsync(request.Id, cancellationToken);

        return Result.Ok();
    }
}