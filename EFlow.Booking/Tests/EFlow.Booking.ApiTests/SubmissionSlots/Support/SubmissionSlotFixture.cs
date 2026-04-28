using EFlow.Booking.ApiTests.Infrastructure.Scenarios;
using EFlow.Booking.ApiTests.Infrastructure.Sessions;

namespace EFlow.Booking.ApiTests.SubmissionSlots.Support;

/// <summary>
/// Holds the shared slot-related entities and sessions used by focused submission-slot tests.
/// </summary>
internal sealed class SubmissionSlotFixture(
    ApiSession adminSession,
    ApiSession teacherSession,
    ApiSession studentSession,
    string suffix,
    Guid groupId,
    string groupName,
    Guid teacherId,
    Guid studentId,
    string studentEmail,
    Guid subjectId,
    string subjectName,
    Guid slotId,
    DateTime slotStart)
{
    /// <summary>
    /// Gets the authenticated administrator session.
    /// </summary>
    public ApiSession AdminSession { get; } = adminSession;

    /// <summary>
    /// Gets the authenticated teacher session.
    /// </summary>
    public ApiSession TeacherSession { get; } = teacherSession;

    /// <summary>
    /// Gets the authenticated student session.
    /// </summary>
    public ApiSession StudentSession { get; } = studentSession;

    /// <summary>
    /// Gets the unique scenario suffix.
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
    /// Gets the created student's email address.
    /// </summary>
    public string StudentEmail { get; } = studentEmail;

    /// <summary>
    /// Gets the created subject identifier.
    /// </summary>
    public Guid SubjectId { get; } = subjectId;

    /// <summary>
    /// Gets the created subject name.
    /// </summary>
    public string SubjectName { get; } = subjectName;

    /// <summary>
    /// Gets the created submission slot identifier.
    /// </summary>
    public Guid SlotId { get; } = slotId;

    /// <summary>
    /// Gets the created submission slot start time.
    /// </summary>
    public DateTime SlotStart { get; } = slotStart;

    /// <summary>
    /// Gets the cleanup actions required for the created entities.
    /// </summary>
    public List<Func<ApiSession, Task>> CleanupActions { get; } =
    [
        ApiScenario.DeleteSlot(slotId),
        ApiScenario.DeleteSubject(subjectId),
        ApiScenario.DeleteStudent(studentId),
        ApiScenario.DeleteTeacher(teacherId),
        ApiScenario.DeleteGroup(groupId)
    ];

    /// <summary>
    /// Adds a cleanup action that should run before the default entity cleanup.
    /// </summary>
    /// <param name="action">Cleanup action to prepend.</param>
    public void AddCleanup(Func<ApiSession, Task> action) =>
        CleanupActions.Insert(0, action);
}