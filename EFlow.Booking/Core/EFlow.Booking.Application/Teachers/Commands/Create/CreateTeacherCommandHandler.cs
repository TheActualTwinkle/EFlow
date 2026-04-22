using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Application.Common.Errors.Identity;
using EFlow.Booking.Domain;
using EFlow.Booking.Domain.Teachers;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EFlow.Booking.Application.Teachers.Commands;

public class CreateTeacherCommandHandler(
    IUnitOfWork unitOfWork,
    UserManager<Identity> userManager,
    ISystemClock systemClock)
    : IRequestHandler<CreateTeacherCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateTeacherCommand request, CancellationToken cancellationToken)
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

        var addToRoleResult = await userManager.AddToRoleAsync(identity, Identity.Roles.Teacher);

        if (!addToRoleResult.Succeeded)
            return Result.Fail(
                new IdentityInternalError()
                    .WithMessage("Failed to add user to role")
                    .WithIdentityErrors(addToRoleResult.Errors));

        var nowUtc = systemClock.UtcNow;

        var teacher = Teacher.Create(
            new TeacherId(identity.Id),
            request.FirstName,
            request.LastName,
            request.MiddleName,
            request.BirthDate,
            nowUtc,
            nowUtc);

        await unitOfWork
            .GetRepository<ITeacherRepository>()
            .CreateAsync(teacher, cancellationToken);

        return Result.Ok(teacher.Id.Value);
    }
}
