using System.Linq.Expressions;
using EFlow.Booking.Contracts.Teachers;
using EFlow.Booking.Domain.Teachers;
using EFlow.Booking.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace EFlow.Booking.Persistence.QueryServices;

public sealed class TeacherQueryService(ApplicationDbContext context) : ITeacherQueryService
{
    public async Task<IEnumerable<TeacherView>> GetAllAsync(CancellationToken cancellationToken = new()) =>
        await context.Teachers
            .Select(MapToView())
            .ToListAsync(cancellationToken);

    public async Task<TeacherView?> GetByIdAsync(TeacherId id, CancellationToken cancellationToken = new()) =>
        await context.Teachers 
            .Where(t => t.Id == id)
            .Select(MapToView())
            .FirstOrDefaultAsync(cancellationToken);

    private static Expression<Func<Teacher, TeacherView>> MapToView() =>
        teacher => new TeacherView
        {
            Id = teacher.Id.Value,
            FirstName = teacher.FirstName,
            LastName = teacher.LastName,
            MiddleName = teacher.MiddleName,
            CreatedAt = teacher.CreatedAt,
            BirthDate = teacher.BirthDate
        };
}
