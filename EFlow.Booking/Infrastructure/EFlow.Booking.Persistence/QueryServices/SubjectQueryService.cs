using System.Linq.Expressions;
using EFlow.Booking.Contracts.Subjects;
using EFlow.Booking.Domain.Subjects;
using EFlow.Booking.Domain.Teachers;
using EFlow.Booking.Persistence.DatabaseContext;
using EFlow.Booking.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace EFlow.Booking.Persistence.QueryServices;

public sealed class SubjectQueryService(ApplicationDbContext context) : ISubjectQueryService
{
    public async Task<IEnumerable<SubjectView>> GetAllAsync(CancellationToken cancellationToken = new()) =>
        await context.Subjects
            .Select(MapToView())
            .ToListAsync(cancellationToken);

    public async Task<SubjectView?> GetByIdAsync(SubjectId id, CancellationToken cancellationToken = new()) =>
        await context.Subjects
            .Where(s => s.Id == id)
            .Select(MapToView())
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IEnumerable<SubjectView>> GetByTeacherIdAsync(TeacherId teacherId, CancellationToken cancellationToken = new()) =>
        await context.Subjects
            .Where(s => s.TeacherId == teacherId)
            .Select(MapToView())
            .ToListAsync(cancellationToken);

    private Expression<Func<Subject, SubjectView>> MapToView() =>
        subject => new SubjectView
        {
            Id = subject.Id.Value,
            Name = subject.Name,
            Teacher = context.Teachers
                .Where(t => t.Id == subject.TeacherId)
                .Select(t => t.ToTeacherView())
                .FirstOrDefault()!,
            Groups = context.Groups
                .Where(g => subject.GroupIds.Contains(g.Id))
                .Select(g => g.ToGroupView())
                .ToList()
        };
}
