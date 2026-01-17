using EFlow.Common.Domain;
using EFlow.Common.Infrastructure;
using FluentResults;
using Mapster;
using MediatR;

namespace EFlow.Booking.Application.Subjects.Queries;

public class GetSubjectsByTeacherIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetSubjectsByTeacherIdQuery, Result<IEnumerable<SubjectDto>>>
{
    public async Task<Result<IEnumerable<SubjectDto>>> Handle(GetSubjectsByTeacherIdQuery request, CancellationToken cancellationToken)
    {
        var subjects = (await unitOfWork
                .GetRepository<ISubjectRepository>()
                .GetByTeacherIdAsync(request.TeacherId, cancellationToken))
            .Adapt<IEnumerable<SubjectDto>>();

        return Result.Ok(subjects);
    }
}