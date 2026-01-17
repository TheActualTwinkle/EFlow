using EFlow.Common.Domain.Models;
using EFlow.Common.Domain;
using EFlow.Booking.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace EFlow.Booking.Persistence.Repositories;

public class SubmissionSlotRepository(ApplicationDbContext context) :
    RepositoryBase<SubmissionSlot>(context), ISubmissionSlotRepository
{
    public async Task CreateAsync(SubmissionSlot slot, CancellationToken cancellationToken = new()) =>
        await CreateInternalAsync(slot, cancellationToken);

    public async Task<IEnumerable<SubmissionSlot>> GetAllAsync(CancellationToken cancellationToken = new()) =>
        await GetAllInternalAsync(cancellationToken);

    public async Task<SubmissionSlot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = new()) =>
        await GetByIdInternalAsync(id, cancellationToken);

    public async Task<IEnumerable<SubmissionSlot>> GetBySubjectIdAsync(Guid subjectId, CancellationToken cancellationToken = new()) =>
        await context.SubmissionSlots
            .Where(s => s.SubjectId == subjectId)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<SubmissionSlot>> GetAvailableSlotsAsync(DateTime fromDate, CancellationToken cancellationToken = new()) =>
        await context.SubmissionSlots
            .Where(s => s.StartTime >= fromDate)
            .ToListAsync(cancellationToken);

    public void Update(SubmissionSlot slot) =>
        UpdateInternal(slot);

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = new()) =>
        DeleteInternalAsync(id, cancellationToken);
}