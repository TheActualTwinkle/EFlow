using EFlow.Booking.Domain.Teachers;
using EFlow.Booking.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace EFlow.Booking.Persistence.Repositories;

public class TeacherRepository(ApplicationDbContext context) :
    ITeacherRepository
{
    public async Task CreateAsync(Teacher teacher, CancellationToken cancellationToken = new()) =>
        await context.Teachers.AddAsync(teacher, cancellationToken);

    public async Task<IEnumerable<Teacher>> GetAllAsync(CancellationToken cancellationToken = new()) =>
        await context.Teachers.ToListAsync(cancellationToken);

    public async Task<Teacher?> GetByIdAsync(TeacherId id, CancellationToken cancellationToken = new()) =>
        await context.Teachers.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public void Update(Teacher teacher) =>
        context.Teachers.Update(teacher);

    public Task DeleteAsync(Teacher teacher)
    {
        context.Teachers.Remove(teacher);
        
        return Task.CompletedTask;
    }
}