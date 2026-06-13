using EFlow.Booking.Application.Common.Errors;
using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Contracts.Students;
using EFlow.Booking.Contracts.Subjects;
using EFlow.Booking.Contracts.SubmissionSlots;
using EFlow.Booking.Domain.BookingRecords;
using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Booking.Domain.Subjects;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.BookingRecords.Queries.GetNotBookedStudents;

public sealed class GetNotBookedStudentsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetNotBookedStudentsQuery, Result<NotBookedStudentsView>>
{
    public async Task<Result<NotBookedStudentsView>> Handle(
        GetNotBookedStudentsQuery request,
        CancellationToken cancellationToken)
    {
        var slotId = new SubmissionSlotId(request.SlotId);

        var slot = await unitOfWork
            .GetQueryService<ISubmissionSlotQueryService>()
            .GetByIdAsync(slotId, cancellationToken);

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

        var allowedStudents = allowedGroupIds.Length == 0 ?
            [] :
            await unitOfWork
                .GetQueryService<IStudentQueryService>()
                .GetByGroupIdsAsync(allowedGroupIds, cancellationToken);
        
        var bookedStudentIds = (await unitOfWork
            .GetRepository<IBookingRecordRepository>()
            .GetBySlotIdAsync(slotId, cancellationToken))
            .Select(b => b.GetStudentId().Value)
            .ToHashSet();

        var admittedStudentIds = slot.AdmittedStudents?
                                     .Select(a => a.Id)
                                     .ToHashSet() ?? [];

        var notBookedStudents = allowedStudents
            .Where(s => !bookedStudentIds.Contains(s.Id))
            .ToList();

        return Result.Ok(
            new NotBookedStudentsView
            {
                AdmittedStudents = notBookedStudents
                    .Where(s => admittedStudentIds.Contains(s.Id))
                    .ToList(),
                NotAdmittedStudents = notBookedStudents
                    .Where(s => !admittedStudentIds.Contains(s.Id))
                    .ToList()
            });
    }
}
