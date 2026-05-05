using EFlow.Booking.Application.Common.Errors;
using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Domain.Teachers;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Teachers.Commands.Update;

public class UpdateTeacherCommandHandler(IUnitOfWork unitOfWork, ISystemClock systemClock) : IRequestHandler<UpdateTeacherCommand, Result>
{
    public async Task<Result> Handle(UpdateTeacherCommand request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.GetRepository<ITeacherRepository>();

        var teacher = await repository.GetByIdAsync(new TeacherId(request.Id), cancellationToken);

        if (teacher is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("Teacher not found")
                    .WithId(request.Id));

        teacher.Update(request.Patch, systemClock.UtcNow);

        repository.Update(teacher);

        return Result.Ok();
    }
}
