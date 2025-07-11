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

    public IEnumerable<Subject> GetAll() =>
        GetAllInternal();

    public async Task<Subject?> GetByIdAsync(Guid id, CancellationToken cancellationToken = new()) =>
        await GetByIdInternalAsync(id, cancellationToken);

    public async Task<IEnumerable<Subject>> GetByTeacherIdAsync(Guid teacherId, CancellationToken cancellationToken = new()) =>
        await Context.Subjects
            .Where(s => s.TeacherId == teacherId)
            .ToListAsync(cancellationToken);

    public void Update(Subject subject) =>
        UpdateInternal(subject);

    public void Delete(Subject subject) =>
        DeleteInternal(subject);
}