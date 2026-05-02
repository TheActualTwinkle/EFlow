using System.Net;
using EFlow.Booking.ApiTests.Infrastructure.Contracts;
using EFlow.Booking.ApiTests.Infrastructure.Fixtures;
using EFlow.Booking.ApiTests.Infrastructure.Sessions;
using EFlow.Booking.Contracts.BookingRecords;
using EFlow.Booking.Contracts.Groups;
using EFlow.Booking.Contracts.Students;
using EFlow.Booking.Contracts.Subjects;
using EFlow.Booking.Contracts.SubmissionSlots;
using EFlow.Booking.Contracts.Teachers;
using EFlow.Booking.WebApi.Contracts.Bookings;
using EFlow.Booking.WebApi.Contracts.Groups;
using EFlow.Booking.WebApi.Contracts.Students;
using EFlow.Booking.WebApi.Contracts.Subjects;
using EFlow.Booking.WebApi.Contracts.SubmissionSlots;
using EFlow.Booking.WebApi.Contracts.Teachers;
using FluentAssertions;

namespace EFlow.Booking.ApiTests.Infrastructure.Scenarios;

/// <summary>
/// Provides high-level setup and cleanup helpers for Booking API end-to-end scenarios.
/// </summary>
internal sealed class ApiScenario(ApiTestStackFixture fixture)
{
    /// <summary>
    /// Gets a unique suffix that keeps resources created by the current scenario isolated.
    /// </summary>
    public string Suffix { get; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>
    /// Authenticates the configured admin user and returns both the session and current-user payload.
    /// </summary>
    public async Task<(ApiSession Session, CurrentUserResponse CurrentUser)> CreateAdminSessionAsync()
    {
        // Arrange
        var session = fixture.CreateSession();

        // Act
        var loginResponse = await session.PostAsync(
            "/api/auth/login",
            new LoginRequestModel
            {
                Username = fixture.AdminUsername,
                Password = fixture.AdminPassword
            });

        // Assert
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var authToken = await session.ReadAsync<AuthTokenResponse>(loginResponse);
        authToken.Should().NotBeNull();
        authToken.Token.Should().NotBeNullOrWhiteSpace();

        var meResponse = await session.GetAsync("/api/auth/me");
        meResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var me = await session.ReadAsync<CurrentUserResponse>(meResponse);
        me.Should().NotBeNull();

        return (session, me);
    }

    /// <summary>
    /// Logs in an existing user and returns a session that holds the authentication cookie.
    /// </summary>
    public async Task<ApiSession> LoginAsync(string username, string password)
    {
        // Arrange
        var session = fixture.CreateSession();

        // Act
        var response = await session.PostAsync(
            "/api/auth/login",
            new LoginRequestModel
            {
                Username = username,
                Password = password
            });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var authToken = await session.ReadAsync<AuthTokenResponse>(response);
        authToken.Should().NotBeNull();
        authToken.Token.Should().NotBeNullOrWhiteSpace();

        return session;
    }

    /// <summary>
    /// Creates a group and returns its identifier.
    /// </summary>
    public async Task<Guid> CreateGroupAsync(ApiSession adminSession, string? name = null)
    {
        var response = await adminSession.PostAsync(
            "/api/groups",
            new CreateGroupRequest
            {
                Name = name ?? $"Group {Suffix}"
            });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        return adminSession.GetCreatedId(response);
    }

    /// <summary>
    /// Creates a teacher account and returns its identifier.
    /// </summary>
    public async Task<Guid> CreateTeacherAsync(
        ApiSession adminSession,
        string username,
        string email,
        string firstName,
        string lastName)
    {
        var response = await adminSession.PostAsync(
            "/api/teachers",
            new CreateTeacherRequest
            {
                UserName = username,
                Password = "Teacher123!",
                Email = email,
                FirstName = firstName,
                MiddleName = "M",
                LastName = lastName,
                BirthDate = new DateOnly(1990, 01, 01)
            });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        return adminSession.GetCreatedId(response);
    }

    /// <summary>
    /// Creates a student account and returns its identifier.
    /// </summary>
    public async Task<Guid> CreateStudentAsync(
        ApiSession adminSession,
        Guid groupId,
        string username,
        string email,
        string firstName,
        string lastName)
    {
        var response = await adminSession.PostAsync(
            "/api/students",
            new CreateStudentRequest
            {
                UserName = username,
                Password = "Student123!",
                Email = email,
                GroupId = groupId,
                FirstName = firstName,
                MiddleName = "M",
                LastName = lastName,
                BirthDate = new DateOnly(2004, 01, 01)
            });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        return adminSession.GetCreatedId(response);
    }

    /// <summary>
    /// Creates a subject linked to the supplied teacher and group and returns its identifier.
    /// </summary>
    public async Task<Guid> CreateSubjectAsync(ApiSession adminSession, Guid teacherId, Guid groupId, string? name = null)
    {
        var response = await adminSession.PostAsync(
            "/api/subjects",
            new CreateSubjectRequest
            {
                Name = name ?? $"Subject {Suffix}",
                TeacherId = teacherId,
                GroupIds = [groupId]
            });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        return adminSession.GetCreatedId(response);
    }

    /// <summary>
    /// Creates a submission slot and returns its identifier.
    /// </summary>
    public async Task<Guid> CreateSlotAsync(
        ApiSession adminSession,
        Guid subjectId,
        Guid teacherId,
        bool allowAllGroups,
        IEnumerable<Guid>? allowedGroupIds = null,
        DateTime? startTime = null)
    {
        var actualStart = startTime ?? DateTime.UtcNow.AddDays(7).Date.AddHours(10);

        var response = await adminSession.PostAsync(
            "/api/submission-slots",
            new CreateSubmissionSlotRequest
            {
                SubjectId = subjectId,
                TeacherId = teacherId,
                StartTime = actualStart,
                EndTime = actualStart.AddHours(1),
                MaxStudents = 5,
                AllowAllGroups = allowAllGroups,
                AllowedGroupIds = allowedGroupIds?.ToArray(),
                Location = "A-101",
                Comment = "API test slot"
            });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        return adminSession.GetCreatedId(response);
    }

    /// <summary>
    /// Adds a student admission to a submission slot and returns the created admission identifier.
    /// </summary>
    public async Task<Guid> AddAdmissionAsync(ApiSession session, Guid slotId, Guid studentId)
    {
        var response = await session.PostAsync<object?>($"/api/submission-slots/{slotId}/admissions/{studentId}", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await session.ReadAsync<Guid>(response);
        payload.Should().NotBeEmpty();

        return payload;
    }

    /// <summary>
    /// Creates a booking and returns its identifier.
    /// </summary>
    public async Task<Guid> CreateBookingAsync(ApiSession session, Guid studentId, Guid slotId)
    {
        var response = await session.PostAsync(
            "/api/bookings",
            new CreateBookingRequest
            {
                StudentId = studentId,
                SlotId = slotId
            });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        return session.GetCreatedId(response);
    }

    /// <summary>
    /// Retrieves a group view by identifier and validates the HTTP response.
    /// </summary>
    public async Task<GroupView> GetGroupAsync(ApiSession session, Guid groupId)
    {
        var response = await session.GetAsync($"/api/groups/{groupId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return (await session.ReadAsync<GroupView>(response))!;
    }

    /// <summary>
    /// Retrieves a teacher view by identifier and validates the HTTP response.
    /// </summary>
    public async Task<TeacherView> GetTeacherAsync(ApiSession session, Guid teacherId)
    {
        var response = await session.GetAsync($"/api/teachers/{teacherId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return (await session.ReadAsync<TeacherView>(response))!;
    }

    /// <summary>
    /// Retrieves a student view by identifier and validates the HTTP response.
    /// </summary>
    public async Task<StudentView> GetStudentAsync(ApiSession session, Guid studentId)
    {
        var response = await session.GetAsync($"/api/students/{studentId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return (await session.ReadAsync<StudentView>(response))!;
    }

    /// <summary>
    /// Retrieves a subject view by identifier and validates the HTTP response.
    /// </summary>
    public async Task<SubjectView> GetSubjectAsync(ApiSession session, Guid subjectId)
    {
        var response = await session.GetAsync($"/api/subjects/{subjectId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return (await session.ReadAsync<SubjectView>(response))!;
    }

    /// <summary>
    /// Retrieves a submission-slot view by identifier and validates the HTTP response.
    /// </summary>
    public async Task<SubmissionSlotView> GetSlotAsync(ApiSession session, Guid slotId)
    {
        var response = await session.GetAsync($"/api/submission-slots/{slotId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return (await session.ReadAsync<SubmissionSlotView>(response))!;
    }

    /// <summary>
    /// Retrieves a booking view by identifier and validates the HTTP response.
    /// </summary>
    public async Task<BookingRecordView> GetBookingAsync(ApiSession session, Guid bookingId)
    {
        var response = await session.GetAsync($"/api/bookings/{bookingId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return (await session.ReadAsync<BookingRecordView>(response))!;
    }

    /// <summary>
    /// Executes cleanup actions sequentially and ignores failures so tests can tear down best-effort resources.
    /// </summary>
    public async Task CleanupAsync(ApiSession adminSession, params Func<ApiSession, Task>[] actions)
    {
        foreach (var action in actions)
            try
            {
                await action(adminSession);
            }
            catch
            {
                // ignored
            }
    }

    /// <summary>
    /// Builds a cleanup action that deletes a booking by identifier.
    /// </summary>
    public static Func<ApiSession, Task> DeleteBooking(Guid id) =>
        session => DeleteResourceAsync(session, $"/api/bookings/{id}");

    /// <summary>
    /// Builds a cleanup action that deletes a submission slot by identifier.
    /// </summary>
    public static Func<ApiSession, Task> DeleteSlot(Guid id) =>
        session => DeleteResourceAsync(session, $"/api/submission-slots/{id}");

    /// <summary>
    /// Builds a cleanup action that deletes a subject by identifier.
    /// </summary>
    public static Func<ApiSession, Task> DeleteSubject(Guid id) =>
        session => DeleteResourceAsync(session, $"/api/subjects/{id}");

    /// <summary>
    /// Builds a cleanup action that deletes a student by identifier.
    /// </summary>
    public static Func<ApiSession, Task> DeleteStudent(Guid id) =>
        session => DeleteResourceAsync(session, $"/api/students/{id}");

    /// <summary>
    /// Builds a cleanup action that deletes a teacher by identifier.
    /// </summary>
    public static Func<ApiSession, Task> DeleteTeacher(Guid id) =>
        session => DeleteResourceAsync(session, $"/api/teachers/{id}");

    /// <summary>
    /// Builds a cleanup action that deletes a group by identifier.
    /// </summary>
    public static Func<ApiSession, Task> DeleteGroup(Guid id) =>
        session => DeleteResourceAsync(session, $"/api/groups/{id}");

    private static async Task DeleteResourceAsync(ApiSession session, string path)
    {
        var response = await session.DeleteAsync(path);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
