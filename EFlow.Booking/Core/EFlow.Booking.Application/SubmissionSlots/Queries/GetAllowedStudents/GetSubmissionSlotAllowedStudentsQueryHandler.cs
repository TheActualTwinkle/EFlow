using EFlow.Booking.Application.Common.Errors;
using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Contracts.Students;
using EFlow.Booking.Contracts.Subjects;
using EFlow.Booking.Contracts.SubmissionSlots;
using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Booking.Domain.Subjects;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Queries.GetAllowedStudents;

public sealed class GetSubmissionSlotAllowedStudentsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetSubmissionSlotAllowedStudentsQuery, Result<IEnumerable<StudentView>>>
{
    public async Task<Result<IEnumerable<StudentView>>> Handle(
        GetSubmissionSlotAllowedStudentsQuery request,
        CancellationToken cancellationToken)
    {
        var slot = await unitOfWork
            .GetQueryService<ISubmissionSlotQueryService>()
            .GetByIdAsync(new SubmissionSlotId(request.SlotId), cancellationToken);

        if (slot is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("Submission slot not found")
                    .WithId(request.SlotId));

        var allowedGroups = slot is { AllowAllGroups: true, Subject: not null }
            ? (await unitOfWork
                .GetQueryService<ISubjectQueryService>()
                .GetByIdAsync(new SubjectId(slot.Subject.Id), cancellationToken))?.Groups
            : slot.AllowedGroups;

        var allowedGroupIds = allowedGroups?
            .Select(group => new GroupId(group.Id))
            .Distinct()
            .ToArray() ?? [];

        if (allowedGroupIds.Length == 0)
            return Result.Ok(Enumerable.Empty<StudentView>());

        var students = await unitOfWork
            .GetQueryService<IStudentQueryService>()
            .GetByGroupIdsAsync(allowedGroupIds, cancellationToken);

        return Result.Ok(students);
    }
}
