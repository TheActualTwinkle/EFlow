using EFlow.Booking.Domain;
using EFlow.Booking.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;

namespace EFlow.Booking.Application.Teachers.Queries;

public class GetAllTeachersQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllTeachersQuery, Result<IEnumerable<TeacherDto>>>
{
    public async Task<Result<IEnumerable<TeacherDto>>> Handle(GetAllTeachersQuery request, CancellationToken cancellationToken)
    {
        var teachers = (await unitOfWork
                .GetRepository<ITeacherRepository>()
                .GetAllAsync(cancellationToken))
            .Adapt<IEnumerable<TeacherDto>>();

        return Result.Ok(teachers);
    }
}