using EFlow.Application.Common.Errors.Abstractions;
using EFlow.Application.Common.Errors.Identity;
using EFlow.Domain;
using EFlow.Domain.Models;
using EFlow.Domain.Repositories;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EFlow.Application.Students.Commands;

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