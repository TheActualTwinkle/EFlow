using EFlow.Booking.Domain.Subjects;
using EFlow.Common.Infrastructure;
using FluentResults;
using Mapster;
using MediatR;

namespace EFlow.Booking.Application.Subjects.Queries;

public class GetAllSubjectsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllSubjectsQuery, Result<IEnumerable<SubjectDto>>>
{
    public async Task<Result<IEnumerable<SubjectDto>>> Handle(GetAllSubjectsQuery request, CancellationToken cancellationToken)
    {
        var subjects = (await unitOfWork
                .GetRepository<ISubjectRepository>()
                .GetAllAsync(cancellationToken))
            .Adapt<IEnumerable<SubjectDto>>();

        return Result.Ok(subjects);
    }
}