using EFlow.Booking.ApiTests.Infrastructure.Scenarios;
using EFlow.Booking.ApiTests.Infrastructure.Sessions;

namespace EFlow.Booking.ApiTests.Bookings.Support;

/// <summary>
/// Holds the shared state required by focused Booking API tests and tracks cleanup actions.
/// </summary>
internal sealed class BookingsFixture(
    ApiSession adminSession,
    ApiSession student1Session,
    Guid groupId,
    Guid teacherId,
    Guid subjectId,
    Guid slotId,
    Guid student1Id,
    Guid student2Id)
{
    /// <summary>
    /// Gets the authenticated administrator session.
    /// </summary>
    public ApiSession AdminSession { get; } = adminSession;

    /// <summary>
    /// Gets the authenticated session for the first student.
    /// </summary>
    public ApiSession Student1Session { get; } = student1Session;

    /// <summary>
    /// Gets the created group identifier.
    /// </summary>
    public Guid GroupId { get; } = groupId;

    /// <summary>
    /// Gets the created teacher identifier.
    /// </summary>
    public Guid TeacherId { get; } = teacherId;

    /// <summary>
    /// Gets the created subject identifier.
    /// </summary>
    public Guid SubjectId { get; } = subjectId;

    /// <summary>
    /// Gets the created submission slot identifier.
    /// </summary>
    public Guid SlotId { get; } = slotId;

    /// <summary>
    /// Gets the first student identifier.
    /// </summary>
    public Guid Student1Id { get; } = student1Id;

    /// <summary>
    /// Gets the second student identifier.
    /// </summary>
    public Guid Student2Id { get; } = student2Id;

    /// <summary>
    /// Gets or sets the first created booking identifier.
    /// </summary>
    public Guid Booking1Id { get; set; }

    /// <summary>
    /// Gets or sets the second created booking identifier.
    /// </summary>
    public Guid Booking2Id { get; set; }

    /// <summary>
    /// Gets the cleanup actions that must run after the test completes.
    /// </summary>
    public List<Func<ApiSession, Task>> CleanupActions { get; } =
    [
        ApiScenario.DeleteSlot(slotId),
        ApiScenario.DeleteSubject(subjectId),
        ApiScenario.DeleteStudent(student1Id),
        ApiScenario.DeleteStudent(student2Id),
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