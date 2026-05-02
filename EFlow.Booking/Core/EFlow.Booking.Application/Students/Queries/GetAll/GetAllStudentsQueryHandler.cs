using EFlow.Booking.Contracts.Students;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Students.Queries;

public class GetAllStudentsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllStudentsQuery, Result<IEnumerable<StudentView>>>
{
    public async Task<Result<IEnumerable<StudentView>>> Handle(GetAllStudentsQuery request, CancellationToken cancellationToken)
    {
        var students = await unitOfWork
            .GetQueryService<IStudentQueryService>()
            .GetAllAsync(cancellationToken);

        return Result.Ok(students);
    }
}