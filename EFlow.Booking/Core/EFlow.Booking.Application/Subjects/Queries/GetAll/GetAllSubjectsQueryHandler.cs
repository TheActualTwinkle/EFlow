using EFlow.Booking.Contracts.Subjects;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Subjects.Queries;

public class GetAllSubjectsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllSubjectsQuery, Result<IEnumerable<SubjectView>>>
{
    public async Task<Result<IEnumerable<SubjectView>>> Handle(GetAllSubjectsQuery request, CancellationToken cancellationToken)
    {
        var subjects = await unitOfWork
            .GetQueryService<ISubjectQueryService>()
            .GetAllAsync(cancellationToken);

        return Result.Ok(subjects);
    }
}