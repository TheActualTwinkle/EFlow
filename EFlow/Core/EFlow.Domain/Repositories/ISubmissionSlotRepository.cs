using EFlow.Domain.Models;

namespace EFlow.Domain.Repositories;

public interface ISubmissionSlotRepository : IRepository
{
    public Task CreateAsync(SubmissionSlot slot, CancellationToken cancellationToken = new());

    public IEnumerable<SubmissionSlot> GetAll();

    public Task<SubmissionSlot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = new());

    public Task<IEnumerable<SubmissionSlot>> GetBySubjectIdAsync(Guid subjectId, CancellationToken cancellationToken = new());

    public Task<IEnumerable<SubmissionSlot>> GetAvailableSlotsAsync(DateTime fromDate, CancellationToken cancellationToken = new());

    public void Update(SubmissionSlot slot);

    public void Delete(SubmissionSlot slot);
}