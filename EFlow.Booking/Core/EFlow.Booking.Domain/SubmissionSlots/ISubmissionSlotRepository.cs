using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.SubmissionSlots;

public interface ISubmissionSlotRepository : IRepository
{
    public Task CreateAsync(SubmissionSlot slot, CancellationToken cancellationToken = new());

    public Task<IEnumerable<SubmissionSlot>> GetAllAsync(CancellationToken cancellationToken = new());

    public Task<SubmissionSlot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = new());

    public Task<IEnumerable<SubmissionSlot>> GetBySubjectIdAsync(Guid subjectId, CancellationToken cancellationToken = new());

    public Task<IEnumerable<SubmissionSlot>> GetAvailableSlotsAsync(DateTime fromDate, CancellationToken cancellationToken = new());

    public void Update(SubmissionSlot slot);

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = new());
}