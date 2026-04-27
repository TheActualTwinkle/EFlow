using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Application.Common.Errors.Identity;
using EFlow.Booking.Domain;
using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Domain.Students;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EFlow.Booking.Application.Students.Commands;

public class CreateStudentCommandHandler(
    IUnitOfWork unitOfWork,
    UserManager<Identity> userManager,
    ISystemClock systemClock)
    : IRequestHandler<CreateStudentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
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

        var addToRoleResult = await userManager.AddToRoleAsync(identity, Identity.Roles.Student);

        if (!addToRoleResult.Succeeded)
            return Result.Fail(
                new IdentityInternalError()
                    .WithMessage("Failed to add user to role")
                    .WithIdentityErrors(addToRoleResult.Errors));

        var nowUtc = systemClock.UtcNow;

        var student = Student.Create(
            new StudentId(identity.Id),
            new GroupId(request.GroupId),
            request.FirstName,
            request.LastName,
            request.MiddleName,
            request.BirthDate,
            nowUtc,
            nowUtc);

        await unitOfWork
            .GetRepository<IStudentRepository>()
            .CreateAsync(student, cancellationToken);

        return Result.Ok(student.Id.Value);
    }
}
