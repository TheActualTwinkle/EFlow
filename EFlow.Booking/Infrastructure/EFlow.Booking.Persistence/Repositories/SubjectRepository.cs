using EFlow.Booking.Persistence.DatabaseContext;
using EFlow.Booking.Domain.Subjects;
using Microsoft.EntityFrameworkCore;

namespace EFlow.Booking.Persistence.Repositories;

public class SubjectRepository(ApplicationDbContext context) :
    RepositoryBase<Subject>(context), ISubjectRepository
{
    public async Task CreateAsync(Subject subject, CancellationToken cancellationToken = new()) =>
        await CreateInternalAsync(subject, cancellationToken);

    public async Task<IEnumerable<Subject>> GetAllAsync(CancellationToken cancellationToken = new()) =>
        await GetAllInternalAsync(cancellationToken);

    public async Task<Subject?> GetByIdAsync(Guid id, CancellationToken cancellationToken = new()) =>
        await context.Subjects.FirstOrDefaultAsync(s => s.Id.Value == id, cancellationToken);

    public async Task<IEnumerable<Subject>> GetByTeacherIdAsync(Guid teacherId, CancellationToken cancellationToken = new()) =>
        await context.Subjects
            .Where(s => s.TeacherId.Value == teacherId)
            .ToListAsync(cancellationToken);

    public void Update(Subject subject) =>
        UpdateInternal(subject);

    public Task DeleteAsync(Subject subject) =>
        DeleteInternalAsync(subject);
}