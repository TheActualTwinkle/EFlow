using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Application.Common.Errors.Identity;
using EFlow.Booking.Domain;
using EFlow.Booking.Domain.Teachers;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EFlow.Booking.Application.Teachers.Commands;

public class DeleteTeacherCommandHandler(IUnitOfWork unitOfWork, UserManager<Identity> userManager) : IRequestHandler<DeleteTeacherCommand, Result>
{
    public async Task<Result> Handle(DeleteTeacherCommand request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.GetRepository<ITeacherRepository>();
        
        var identity = await userManager.FindByIdAsync(request.Id.ToString());

        if (identity is null)
            return Result.Ok();

        var result = await userManager.DeleteAsync(identity);

        if (!result.Succeeded)
            return Result.Fail(
                new IdentityInternalError()
                    .WithMessage("Failed to delete teacher")
                    .WithIdentityErrors(result.Errors));

        var teacher = await repository.GetByIdAsync(new TeacherId(request.Id), cancellationToken);

        if (teacher is null)
            return Result.Ok();

        teacher.Delete();

        await repository.DeleteAsync(teacher);

        return Result.Ok();
    }
}