using EFlow.Booking.Application.Common.Errors;
using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Domain.Students;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Students.Commands.Update;

public class UpdateStudentCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateStudentCommand, Result>
{
    public async Task<Result> Handle(UpdateStudentCommand request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.GetRepository<IStudentRepository>();

        var student = await repository.GetByIdAsync(new StudentId(request.Id), cancellationToken);

        if (student is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("Student not found")
                    .WithId(request.Id));

        // TODO: Update Domain Model

        repository.Update(student);

        return Result.Ok();
    }
}
