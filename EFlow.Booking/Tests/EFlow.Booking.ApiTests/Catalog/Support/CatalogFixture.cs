using EFlow.Booking.ApiTests.Infrastructure.Scenarios;
using EFlow.Booking.ApiTests.Infrastructure.Sessions;

namespace EFlow.Booking.ApiTests.Catalog.Support;

/// <summary>
/// Holds the shared catalog entities used by focused catalog endpoint tests.
/// </summary>
internal sealed class CatalogFixture(
    ApiSession adminSession,
    string suffix,
    Guid groupId,
    string groupName,
    Guid teacherId,
    Guid studentId,
    Guid subjectId,
    string subjectName)
{
    /// <summary>
    /// Gets the authenticated administrator session.
    /// </summary>
    public ApiSession AdminSession { get; } = adminSession;

    /// <summary>
    /// Gets the scenario-specific suffix used for unique entity naming.
    /// </summary>
    public string Suffix { get; } = suffix;

    /// <summary>
    /// Gets the created group identifier.
    /// </summary>
    public Guid GroupId { get; } = groupId;

    /// <summary>
    /// Gets the created group name.
    /// </summary>
    public string GroupName { get; } = groupName;

    /// <summary>
    /// Gets the created teacher identifier.
    /// </summary>
    public Guid TeacherId { get; } = teacherId;

    /// <summary>
    /// Gets the created student identifier.
    /// </summary>
    public Guid StudentId { get; } = studentId;

    /// <summary>
    /// Gets the created subject identifier.
    /// </summary>
    public Guid SubjectId { get; } = subjectId;

    /// <summary>
    /// Gets the created subject name.
    /// </summary>
    public string SubjectName { get; } = subjectName;

    /// <summary>
    /// Gets the cleanup actions for the created catalog entities.
    /// </summary>
    public IReadOnlyList<Func<ApiSession, Task>> CleanupActions { get; } =
    [
        ApiScenario.DeleteSubject(subjectId),
        ApiScenario.DeleteStudent(studentId),
        ApiScenario.DeleteTeacher(teacherId),
        ApiScenario.DeleteGroup(groupId)
    ];
}