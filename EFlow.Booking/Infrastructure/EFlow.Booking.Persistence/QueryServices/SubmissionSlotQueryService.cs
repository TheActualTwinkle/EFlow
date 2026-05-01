using System.Linq.Expressions;
using EFlow.Booking.Contracts.SubmissionSlots;
using EFlow.Booking.Domain.Subjects;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Booking.Persistence.DatabaseContext;
using EFlow.Booking.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace EFlow.Booking.Persistence.QueryServices;

public sealed class SubmissionSlotQueryService(ApplicationDbContext context) : ISubmissionSlotQueryService
{
    public async Task<IEnumerable<SubmissionSlotView>> GetAllAsync(CancellationToken cancellationToken = new()) =>
        await context.SubmissionSlots
            .Select(MapToView())
            .ToListAsync(cancellationToken);

    public async Task<SubmissionSlotView?> GetByIdAsync(SubmissionSlotId id, CancellationToken cancellationToken = new()) =>
        await context.SubmissionSlots
            .Where(s => s.Id == id)
            .Select(MapToView())
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IEnumerable<SubmissionSlotView>> GetBySubjectIdAsync(SubjectId subjectId, CancellationToken cancellationToken = new()) =>
        await context.SubmissionSlots
            .Where(s => s.SubjectId == subjectId)
            .Select(MapToView())
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<SubmissionSlotView>> GetAvailableSlotsAsync(DateTime fromDate, CancellationToken cancellationToken = new()) =>
        await context.SubmissionSlots
            .Where(s => s.StartTime >= fromDate)
            // TODO: filter out slots that are already fully booked
            .Select(MapToView())
            .ToListAsync(cancellationToken);

    private Expression<Func<SubmissionSlot, SubmissionSlotView>> MapToView() =>
        submissionSlot => new SubmissionSlotView
        {
            Id = submissionSlot.Id.Value,
            StartTime = submissionSlot.StartTime,
            EndTime = submissionSlot.EndTime,
            MaxStudents = submissionSlot.MaxStudents,
            AllowAllGroups = submissionSlot.AllowAllGroups,
            Location = submissionSlot.Location,
            Comment = submissionSlot.Comment,
            Subject = context.Subjects
                .Where(s => s.Id == submissionSlot.SubjectId)
                .Select(s => s.ToSubjectView())
                .FirstOrDefault()!,
            Teacher = context.Teachers
                .Where(t => t.Id == submissionSlot.TeacherId)
                .Select(t => t.ToTeacherView())
                .FirstOrDefault()!,
            AdmittedStudents = context.Students
                .Where(student => submissionSlot.Admissions
                    .Select(admission => admission.StudentId)
                    .Contains(student.Id))
                .Select(student => student.ToStudentView())
                .ToList(),
            AllowedGroups = context.Groups
                .Where(group => submissionSlot.AllowedGroupIds.Contains(group.Id))
                .Select(group => group.ToGroupView())
                .ToList()
        };
}
