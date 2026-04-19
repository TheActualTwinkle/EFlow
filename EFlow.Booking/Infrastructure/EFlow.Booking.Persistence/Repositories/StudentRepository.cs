using EFlow.Booking.Domain.Students;
using EFlow.Booking.Persistence.DatabaseContext;
using EFlow.Common.Domain.Students;
using Microsoft.EntityFrameworkCore;

namespace EFlow.Booking.Persistence.Repositories;

public class StudentRepository(ApplicationDbContext context) :
    RepositoryBase<Student>(context), IStudentRepository
{
    public async Task CreateAsync(Student student, CancellationToken cancellationToken = new()) =>
        await CreateInternalAsync(student, cancellationToken);

    public async Task<IEnumerable<Student>> GetAllAsync(CancellationToken cancellationToken = new()) =>
        await context.Students.ToListAsync(cancellationToken);

    public async Task<Student?> GetByIdAsync(Guid id, CancellationToken cancellationToken = new()) =>
        await context.Students.FirstOrDefaultAsync(s => s.Id.Value == id, cancellationToken);

    public async Task<IEnumerable<Student>> GetByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = new()) =>
        await context.Students
            .Where(s => s.GroupId.Value == groupId)
            .ToListAsync(cancellationToken);

    public void Update(Student student) =>
        UpdateInternal(student);

    public Task DeleteAsync(Student student) =>
        DeleteInternalAsync(student);
}