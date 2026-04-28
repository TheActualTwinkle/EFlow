using EFlow.Booking.Application.Common.Errors;
using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Domain.Subjects;
using EFlow.Common.Infrastructure;
using FluentResults;
using Mapster;
using MediatR;

namespace EFlow.Booking.Application.Subjects.Queries;

public class GetSubjectByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetSubjectByIdQuery, Result<SubjectDto>>
{
    public async Task<Result<SubjectDto>> Handle(GetSubjectByIdQuery request, CancellationToken cancellationToken)
    {
        var subject = await unitOfWork
            .GetRepository<ISubjectRepository>()
            .GetByIdAsync(new SubjectId(request.Id), cancellationToken);

        if (subject is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("Subject not found")
                    .WithId(request.Id));

        return subject.Adapt<SubjectDto>();
    }
}