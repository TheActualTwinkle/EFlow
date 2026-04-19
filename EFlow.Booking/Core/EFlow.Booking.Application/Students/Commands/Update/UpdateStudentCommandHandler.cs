using EFlow.Booking.Application.Common.Errors;
using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Domain.Students;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Students.Commands.Update;

public class UpdateStudentCommandHandler(IUnitOfWork unitOfWork, ISystemClock systemClock)
    : IRequestHandler<UpdateStudentCommand, Result>
{
    public async Task<Result> Handle(UpdateStudentCommand request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.GetRepository<IStudentRepository>();

        var student = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (student is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("Student not found")
                    .WithId(request.Id));

        // var utcNow = systemClock.UtcNow;

        // student.Update(
        //     request.FirstName,
        //     request.LastName,
        //     request.MiddleName,
        //     request.BirthDate,
        //     utcNow);

        repository.Update(student);

        return Result.Ok();
    }
}