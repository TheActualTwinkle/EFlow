using EFlow.Booking.Application.Common.Errors;
using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Contracts.Students;
using EFlow.Booking.Domain.Students;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Students.Queries;

public class GetStudentByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetStudentByIdQuery, Result<StudentView>>
{
    public async Task<Result<StudentView>> Handle(GetStudentByIdQuery request, CancellationToken cancellationToken)
    {
        var student = await unitOfWork
            .GetQueryService<IStudentQueryService>()
            .GetByIdAsync(new StudentId(request.Id), cancellationToken);

        if (student is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("Student not found")
                    .WithId(request.Id));

        return Result.Ok(student);
    }
}