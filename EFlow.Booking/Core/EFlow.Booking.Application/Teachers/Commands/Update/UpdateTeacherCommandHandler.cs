using EFlow.Booking.Application.Common.Errors;
using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Domain.Teachers;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Teachers.Commands;

public class UpdateTeacherCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateTeacherCommand, Result>
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

        // TODO: Update Domain Model

        repository.Update(teacher);

        return Result.Ok();
    }
}
