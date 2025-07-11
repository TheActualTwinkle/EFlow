using EFlow.Domain.Models;
using EFlow.Domain.Repositories;
using EFlow.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace EFlow.Persistence.Repositories;

public class SubmissionSlotRepository(ApplicationDbContext context) :
    RepositoryBase<SubmissionSlot>(context), ISubmissionSlotRepository
{
    public async Task CreateAsync(SubmissionSlot slot, CancellationToken cancellationToken = new()) =>
        await CreateInternalAsync(slot, cancellationToken);

    public IEnumerable<SubmissionSlot> GetAll() =>
        GetAllInternal();

    public async Task<SubmissionSlot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = new()) =>
        await GetByIdInternalAsync(id, cancellationToken);

    public async Task<IEnumerable<SubmissionSlot>> GetBySubjectIdAsync(Guid subjectId, CancellationToken cancellationToken = new()) =>
        await Context.SubmissionSlots
            .Where(s => s.SubjectId == subjectId)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<SubmissionSlot>> GetAvailableSlotsAsync(DateTime fromDate, CancellationToken cancellationToken = new()) =>
        await Context.SubmissionSlots
            .Where(s => s.StartTime >= fromDate)
            .ToListAsync(cancellationToken);

    public void Update(SubmissionSlot slot) =>
        UpdateInternal(slot);

    public void Delete(SubmissionSlot slot) =>
        DeleteInternal(slot);
}