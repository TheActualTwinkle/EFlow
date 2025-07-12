using EFlow.Application.Common.Errors.Abstractions;
using EFlow.Application.Common.Errors.Identity;
using EFlow.Domain;
using EFlow.Domain.Models;
using EFlow.Domain.Repositories;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EFlow.Application.Teachers.Commands;

public class CreateTeacherCommandHandler(
    IUnitOfWork unitOfWork,
    UserManager<Identity> userManager)
    : IRequestHandler<CreateTeacherCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateTeacherCommand request, CancellationToken cancellationToken)
    {
        var identity = new Identity { UserName = request.UserName };
        var createUserResult = await userManager.CreateAsync(identity, request.Password);

        if (!createUserResult.Succeeded)
            return Result.Fail(
                new IdentityInternalError()
                    .WithMessage("Failed to create user")
                    .WithIdentityErrors(createUserResult.Errors));

        var addToRoleResult = await userManager.AddToRoleAsync(identity, Identity.Roles.Teacher);

        if (!addToRoleResult.Succeeded)
            return Result.Fail(
                new IdentityInternalError()
                    .WithMessage("Failed to add user to role")
                    .WithIdentityErrors(addToRoleResult.Errors));

        var teacher = new Teacher
        {
            Id = identity.Id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName,
            BirthDate = request.BirthDate,
            CreatedAt = request.CreatedAt ?? DateTime.UtcNow
        };

        await unitOfWork
            .GetRepository<ITeacherRepository>()
            .CreateAsync(teacher, cancellationToken);

        return Result.Ok(identity.Id);
    }
}