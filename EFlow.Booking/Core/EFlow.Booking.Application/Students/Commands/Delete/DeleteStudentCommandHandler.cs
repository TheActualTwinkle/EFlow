using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Application.Common.Errors.Identity;
using EFlow.Booking.Domain;
using EFlow.Booking.Domain.Students;
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
        var repository = unitOfWork.GetRepository<IStudentRepository>();
        
        var identity = await userManager.FindByIdAsync(request.Id.ToString());

        if (identity is null)
            return Result.Ok();

        var result = await userManager.DeleteAsync(identity);

        if (!result.Succeeded)
            return Result.Fail(
                new IdentityInternalError()
                    .WithMessage("Failed to delete student")
                    .WithIdentityErrors(result.Errors));

        var student = await repository.GetByIdAsync(new StudentId(request.Id), cancellationToken);

        if (student is null)
            return Result.Ok();

        student.Delete();

        await repository.DeleteAsync(student);

        return Result.Ok();
    }
}