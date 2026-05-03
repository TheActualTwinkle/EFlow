using EFlow.Booking.Domain.Subjects;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Booking.Domain.Teachers;
using EFlow.Common.Domain;

namespace EFlow.Booking.Contracts.SubmissionSlots;

public interface ISubmissionSlotQueryService : IQueryService
{
    public Task<IEnumerable<SubmissionSlotView>> GetAllAsync(CancellationToken cancellationToken = new());

    public Task<SubmissionSlotView?> GetByIdAsync(SubmissionSlotId id, CancellationToken cancellationToken = new());

    public Task<IEnumerable<SubmissionSlotView>> GetBySubjectIdAsync(SubjectId subjectId, CancellationToken cancellationToken = new());
    
    public Task<IEnumerable<SubmissionSlotView>> GetByTeacherIdAsync(TeacherId teacherId, CancellationToken cancellationToken = new());

    public Task<IEnumerable<SubmissionSlotView>> GetAvailableSlotsAsync(DateTime fromDate, CancellationToken cancellationToken = new());
}