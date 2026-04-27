using EFlow.Booking.Persistence.DatabaseContext;
using EFlow.Booking.Domain.Subjects;
using EFlow.Booking.Domain.Teachers;
using Microsoft.EntityFrameworkCore;

namespace EFlow.Booking.Persistence.Repositories;

public class SubjectRepository(ApplicationDbContext context) :
    ISubjectRepository
{
    public async Task CreateAsync(Subject subject, CancellationToken cancellationToken = new()) =>
        await context.Subjects.AddAsync(subject, cancellationToken);

    public async Task<IEnumerable<Subject>> GetAllAsync(CancellationToken cancellationToken = new()) =>
        await context.Subjects.ToListAsync(cancellationToken);

    public async Task<Subject?> GetByIdAsync(SubjectId id, CancellationToken cancellationToken = new()) =>
        await context.Subjects.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<IEnumerable<Subject>> GetByTeacherIdAsync(TeacherId teacherId, CancellationToken cancellationToken = new()) =>
        await context.Subjects
            .Where(s => s.TeacherId == teacherId)
            .ToListAsync(cancellationToken);

    public void Update(Subject subject) =>
        context.Subjects.Update(subject);

    public Task DeleteAsync(Subject subject)
    {
        context.Subjects.Remove(subject);
        
        return Task.CompletedTask;
    }
}