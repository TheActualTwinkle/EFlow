using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Booking.Domain.Subjects;
using EFlow.Booking.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace EFlow.Booking.Persistence.Repositories;

public class SubmissionSlotRepository(ApplicationDbContext context) :
    ISubmissionSlotRepository
{
    public async Task CreateAsync(SubmissionSlot slot, CancellationToken cancellationToken = new()) =>
        await context.SubmissionSlots.AddAsync(slot, cancellationToken);

    public async Task<IEnumerable<SubmissionSlot>> GetAllAsync(CancellationToken cancellationToken = new()) =>
        await context.SubmissionSlots
            .Include(slot => slot.Admissions)
            .Include(slot => slot.NotificationSettings)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

    public async Task<SubmissionSlot?> GetByIdAsync(SubmissionSlotId id, CancellationToken cancellationToken = new()) =>
        await context.SubmissionSlots
            .Include(slot => slot.Admissions)
            .Include(slot => slot.NotificationSettings)
            .AsSplitQuery()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<IEnumerable<SubmissionSlot>> GetBySubjectIdAsync(SubjectId subjectId, CancellationToken cancellationToken = new()) =>
        await context.SubmissionSlots
            .Include(slot => slot.Admissions)
            .Include(slot => slot.NotificationSettings)
            .AsSplitQuery()
            .Where(s => s.SubjectId == subjectId)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<SubmissionSlot>> GetAvailableSlotsAsync(DateTime fromDate, CancellationToken cancellationToken = new()) =>
        await context.SubmissionSlots
            .Include(slot => slot.Admissions)
            .Include(slot => slot.NotificationSettings)
            .AsSplitQuery()
            .Where(s => s.StartTime >= fromDate)
            .ToListAsync(cancellationToken);

    public void Update(SubmissionSlot slot) =>
        context.SubmissionSlots.Update(slot);

    public Task DeleteAsync(SubmissionSlot submissionSlot)
    {
        context.SubmissionSlots.Remove(submissionSlot);
        
        return Task.CompletedTask;
    }
}
