using EFlow.Booking.Contracts.Teachers;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Teachers.Queries;

public class GetAllTeachersQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllTeachersQuery, Result<IEnumerable<TeacherView>>>
{
    public async Task<Result<IEnumerable<TeacherView>>> Handle(GetAllTeachersQuery request, CancellationToken cancellationToken)
    {
        var teachers = await unitOfWork
            .GetQueryService<ITeacherQueryService>()
            .GetAllAsync(cancellationToken);

        return Result.Ok(teachers);
    }
}