using EFlow.Booking.Contracts.Students;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Students.Queries;

public sealed class GetByGroupIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetByGroupIdQuery, Result<IEnumerable<StudentView>>>
{
    public async Task<Result<IEnumerable<StudentView>>> Handle(GetByGroupIdQuery request, CancellationToken cancellationToken)
    {
        var students = await unitOfWork
            .GetQueryService<IStudentQueryService>()
            .GetByGroupIdAsync(request.GroupId, cancellationToken);

        return Result.Ok(students);
    }
}