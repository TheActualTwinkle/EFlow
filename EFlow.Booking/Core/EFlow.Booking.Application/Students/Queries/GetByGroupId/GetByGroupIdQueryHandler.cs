using EFlow.Booking.Domain.Students;
using EFlow.Common.Infrastructure;
using FluentResults;
using Mapster;
using MediatR;

namespace EFlow.Booking.Application.Students.Queries;

public sealed class GetByGroupIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetByGroupIdQuery, Result<IEnumerable<StudentDto>>>
{
    public async Task<Result<IEnumerable<StudentDto>>> Handle(GetByGroupIdQuery request, CancellationToken cancellationToken)
    {
        var students = (await unitOfWork
                .GetRepository<IStudentRepository>()
                .GetByGroupIdAsync(request.GroupId, cancellationToken))
            .Adapt<IEnumerable<StudentDto>>();

        return Result.Ok(students);
    }
}