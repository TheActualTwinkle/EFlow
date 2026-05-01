using EFlow.Booking.Contracts.Subjects;
using EFlow.Booking.Domain.Teachers;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Subjects.Queries;

public class GetSubjectsByTeacherIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetSubjectsByTeacherIdQuery, Result<IEnumerable<SubjectView>>>
{
    public async Task<Result<IEnumerable<SubjectView>>> Handle(GetSubjectsByTeacherIdQuery request, CancellationToken cancellationToken)
    {
        var subjects = (await unitOfWork
            .GetQueryService<ISubjectQueryService>()
            .GetByTeacherIdAsync(new TeacherId(request.TeacherId), cancellationToken));

        return Result.Ok(subjects);
    }
}