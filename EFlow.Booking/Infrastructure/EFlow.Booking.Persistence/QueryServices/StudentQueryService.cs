using System.Linq.Expressions;
using EFlow.Booking.Contracts.Students;
using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Domain.Students;
using EFlow.Booking.Persistence.DatabaseContext;
using EFlow.Booking.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace EFlow.Booking.Persistence.QueryServices;

public sealed class StudentQueryService(ApplicationDbContext context) : IStudentQueryService
{
    public async Task<IEnumerable<StudentView>> GetAllAsync(CancellationToken cancellationToken = new()) =>
        await context.Students
            .Select(MapToView())
            .ToListAsync(cancellationToken);

    public async Task<StudentView?> GetByIdAsync(StudentId id, CancellationToken cancellationToken = new()) =>
        await context.Students
            .Where(s => s.Id == id)
            .Select(MapToView())
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IEnumerable<StudentView>> GetByGroupIdAsync(GroupId groupId, CancellationToken cancellationToken = new()) =>
        await context.Students
            .Where(s => s.GroupId == groupId)
            .Select(MapToView())
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<StudentView>> GetByGroupIdsAsync(
        IEnumerable<GroupId> groupId,
        CancellationToken cancellationToken = new()) =>
        await context.Students
            .Where(s => groupId.Contains(s.GroupId))
            .Select(MapToView())
            .ToListAsync(cancellationToken);

    private Expression<Func<Student, StudentView>> MapToView() =>
        student => new StudentView
        {
            Id = student.Id.Value,
            FirstName = student.FirstName,
            LastName = student.LastName,
            MiddleName = student.MiddleName,
            BirthDate = student.BirthDate,
            CreatedAt = student.CreatedAt,
            Group = context.Groups
                .Where(g => g.Id == student.GroupId)
                .Select(g => g.ToGroupView())
                .FirstOrDefault()!
        };
}
