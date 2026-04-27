using EFlow.Booking.Domain.Students;
using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace EFlow.Booking.Persistence.Repositories;

public class StudentRepository(ApplicationDbContext context) :
    IStudentRepository
{
    public async Task CreateAsync(Student student, CancellationToken cancellationToken = new()) =>
        await context.Students.AddAsync(student, cancellationToken);

    public async Task<IEnumerable<Student>> GetAllAsync(CancellationToken cancellationToken = new()) =>
        await context.Students.ToListAsync(cancellationToken);

    public async Task<Student?> GetByIdAsync(StudentId id, CancellationToken cancellationToken = new()) =>
        await context.Students.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<IEnumerable<Student>> GetByGroupIdAsync(GroupId groupId, CancellationToken cancellationToken = new()) =>
        await context.Students
            .Where(s => s.GroupId == groupId)
            .ToListAsync(cancellationToken);

    public void Update(Student student) =>
        context.Students.Update(student);

    public Task DeleteAsync(Student student)
    {
        context.Students.Remove(student);
        
        return Task.CompletedTask;
    }
}