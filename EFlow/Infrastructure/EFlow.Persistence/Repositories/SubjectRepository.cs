using EFlow.Domain.Models;
using EFlow.Domain.Repositories;
using EFlow.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace EFlow.Persistence.Repositories;

public class SubjectRepository(ApplicationDbContext context) :
    RepositoryBase<Subject>(context), ISubjectRepository
{
    public async Task CreateAsync(Subject subject, CancellationToken cancellationToken = new()) =>
        await CreateInternalAsync(subject, cancellationToken);

    public async Task<IEnumerable<Subject>> GetAllAsync(CancellationToken cancellationToken = new()) =>
        await Context.Subjects
            .Include(s => s.Teacher)
            .ToListAsync(cancellationToken);

    public async Task<Subject?> GetByIdAsync(Guid id, CancellationToken cancellationToken = new()) =>
        await Context.Subjects
            .Include(s => s.Teacher)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<IEnumerable<Subject>> GetByTeacherIdAsync(Guid teacherId, CancellationToken cancellationToken = new()) =>
        await Context.Subjects
            .Where(s => s.TeacherId == teacherId)
            .Include(s => s.Teacher)
            .ToListAsync(cancellationToken);

    public void Update(Subject subject) =>
        UpdateInternal(subject);

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = new()) =>
        DeleteInternalAsync(id, cancellationToken);
}