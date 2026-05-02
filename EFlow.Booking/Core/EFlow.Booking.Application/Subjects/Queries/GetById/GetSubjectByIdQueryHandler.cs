using EFlow.Booking.Application.Common.Errors;
using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Contracts.Subjects;
using EFlow.Booking.Domain.Subjects;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Subjects.Queries;

public class GetSubjectByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetSubjectByIdQuery, Result<SubjectView>>
{
    public async Task<Result<SubjectView>> Handle(GetSubjectByIdQuery request, CancellationToken cancellationToken)
    {
        var subject = await unitOfWork
            .GetQueryService<ISubjectQueryService>()
            .GetByIdAsync(new SubjectId(request.Id), cancellationToken);

        if (subject is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("Subject not found")
                    .WithId(request.Id));

        return Result.Ok(subject);
    }
}