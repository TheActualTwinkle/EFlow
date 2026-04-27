using EFlow.Common.Domain;
using EFlow.Booking.Domain.Subjects;

namespace EFlow.Booking.Domain.SubmissionSlots;

public interface ISubmissionSlotRepository : IRepository
{
    public Task CreateAsync(SubmissionSlot slot, CancellationToken cancellationToken = new());

    public Task<IEnumerable<SubmissionSlot>> GetAllAsync(CancellationToken cancellationToken = new());

    public Task<SubmissionSlot?> GetByIdAsync(SubmissionSlotId id, CancellationToken cancellationToken = new());

    public Task<IEnumerable<SubmissionSlot>> GetBySubjectIdAsync(SubjectId subjectId, CancellationToken cancellationToken = new());

    public Task<IEnumerable<SubmissionSlot>> GetAvailableSlotsAsync(DateTime fromDate, CancellationToken cancellationToken = new());

    public void Update(SubmissionSlot slot);

    public Task DeleteAsync(SubmissionSlot submissionSlot);
}