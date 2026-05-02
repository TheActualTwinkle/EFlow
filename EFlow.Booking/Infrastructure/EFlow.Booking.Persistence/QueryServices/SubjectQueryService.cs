using EFlow.Booking.Contracts.Subjects;
using EFlow.Booking.Domain.Subjects;
using EFlow.Booking.Domain.Teachers;
using EFlow.Booking.Persistence.DatabaseContext;
using EFlow.Booking.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace EFlow.Booking.Persistence.QueryServices;

public sealed class SubjectQueryService(ApplicationDbContext context) : ISubjectQueryService
{
    public async Task<IEnumerable<SubjectView>> GetAllAsync(CancellationToken cancellationToken = new())
    {
        var subjects = await context.Subjects
            .AsNoTracking()
            .Include(subject => subject.GroupIds)
            .ToListAsync(cancellationToken);

        return await MapToViewsAsync(subjects, cancellationToken);
    }

    public async Task<SubjectView?> GetByIdAsync(SubjectId id, CancellationToken cancellationToken = new())
    {
        var subjects = await context.Subjects
            .AsNoTracking()
            .Include(subject => subject.GroupIds)
            .Where(subject => subject.Id == id)
            .ToListAsync(cancellationToken);

        return (await MapToViewsAsync(subjects, cancellationToken)).FirstOrDefault();
    }

    public async Task<IEnumerable<SubjectView>> GetByTeacherIdAsync(TeacherId teacherId, CancellationToken cancellationToken = new())
    {
        var subjects = await context.Subjects
            .AsNoTracking()
            .Include(subject => subject.GroupIds)
            .Where(subject => subject.TeacherId == teacherId)
            .ToListAsync(cancellationToken);

        return await MapToViewsAsync(subjects, cancellationToken);
    }

    private async Task<IReadOnlyCollection<SubjectView>> MapToViewsAsync(
        IReadOnlyCollection<Subject> subjects,
        CancellationToken cancellationToken)
    {
        if (subjects.Count == 0)
            return [];

        var teacherIds = subjects
            .Select(subject => subject.TeacherId)
            .Distinct()
            .ToArray();

        var groupIds = subjects
            .SelectMany(subject => subject.GroupIds)
            .Distinct()
            .ToArray();

        var teachers = await context.Teachers
            .AsNoTracking()
            .Where(teacher => teacherIds.AsEnumerable().Contains(teacher.Id))
            .ToDictionaryAsync(teacher => teacher.Id, cancellationToken);

        var groups = await context.Groups
            .AsNoTracking()
            .Where(group => groupIds.AsEnumerable().Contains(group.Id))
            .ToDictionaryAsync(group => group.Id, cancellationToken);

        return subjects
            .Select(subject =>
            {
                teachers.TryGetValue(subject.TeacherId, out var teacher);

                var subjectGroups = subject.GroupIds
                    .Where(groups.ContainsKey)
                    .Select(groupId => groups[groupId]);

                return subject.ToSubjectView(teacher, subjectGroups);
            })
            .ToArray();
    }
}
