using EFlow.Domain.Models;
using EFlow.Domain.Repositories;
using EFlow.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace EFlow.Persistence.Repositories;

public class StudentRepository(ApplicationDbContext context) :
    RepositoryBase<Student>(context), IStudentRepository
{
    public async Task CreateAsync(Student student, CancellationToken cancellationToken = new()) =>
        await CreateInternalAsync(student, cancellationToken);

    public async Task<IEnumerable<Student>> GetAllAsync(CancellationToken cancellationToken = new()) =>
        await GetAllInternalAsync(cancellationToken);

    public async Task<Student?> GetByIdAsync(Guid id, CancellationToken cancellationToken = new()) =>
        await GetByIdInternalAsync(id, cancellationToken);

    public async Task<IEnumerable<Student>> GetByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = new()) =>
        await Context.Students
            .Where(s => s.GroupId == groupId)
            .ToListAsync(cancellationToken);

    public void Update(Student student) =>
        UpdateInternal(student);

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = new()) =>
        DeleteInternalAsync(id, cancellationToken);
}