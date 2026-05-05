using System.Net;
using EFlow.Booking.ApiTests.Infrastructure.Assertions;
using EFlow.Booking.ApiTests.Infrastructure.Collections;
using EFlow.Booking.ApiTests.Infrastructure.Fixtures;
using EFlow.Booking.ApiTests.Infrastructure.Scenarios;
using EFlow.Booking.ApiTests.Infrastructure.Sessions;
using EFlow.Booking.ApiTests.SubmissionSlots.Support;
using EFlow.Booking.Contracts.SubmissionSlots;
using EFlow.Booking.Application.SubmissionSlots;
using EFlow.Booking.Domain.Notifications;
using EFlow.Common.IntegrationEvents.Booking.Models;
using FluentAssertions;

namespace EFlow.Booking.ApiTests.SubmissionSlots;

/// <summary>
/// Verifies submission-slot endpoints for reads, permissions, and reminder settings.
/// </summary>
[Collection(ApiTestCollection.Name)]
public sealed class SubmissionSlotsApiTests(ApiTestStackFixture fixture)
{
    /// <summary>
    /// Verifies that <c>GetSubmissionSlots</c> returns the created slot when the slot exists.
    /// </summary>
    [Fact]
    public Task GetSubmissionSlots_WhenSlotExists_ShouldReturnCreatedSlot() =>
        WithSubmissionSlotFixtureAsync(async (scenario, context) =>
        {
            // Arrange
            var slot = await scenario.GetSlotAsync(context.AdminSession, context.SlotId);

            // Act
            var bySubjectResponse = await context.AdminSession.GetAsync($"/api/submission-slots/by-subject/{context.SubjectId}");
            var availableResponse = await context.AdminSession.GetAsync(
                $"/api/submission-slots/available?fromDate={Uri.EscapeDataString(DateTime.UtcNow.Date.ToString("O"))}");

            // Assert
            slot.Id.Should().Be(context.SlotId);
            slot.Subject.Should().NotBeNull();
            slot.Subject!.Id.Should().Be(context.SubjectId);
            slot.Teacher.Should().NotBeNull();
            slot.Teacher!.Id.Should().Be(context.TeacherId);
            slot.StartTime.Should().Be(context.SlotStart);
            slot.EndTime.Should().Be(context.SlotStart.AddHours(1));
            slot.MaxStudents.Should().Be(5);
            slot.Location.Should().Be("A-101");
            slot.Comment.Should().Be("API test slot");
            bySubjectResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            availableResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var slotsBySubject = (await context.AdminSession.ReadAsync<SubmissionSlotView[]>(bySubjectResponse))!;
            var availableSlots = (await context.AdminSession.ReadAsync<SubmissionSlotView[]>(availableResponse))!;
            slotsBySubject.Should().Contain(x => x.Id == context.SlotId && x.Subject != null && x.Subject.Id == context.SubjectId);
            availableSlots.Should().Contain(x => x.Id == context.SlotId);
        });

    /// <summary>
    /// Verifies that <c>UpdateSubmissionSlot</c> replaces the allowed groups stored for the slot.
    /// </summary>
    [Fact]
    public Task UpdateSubmissionSlot_WhenAllowedGroupIdsChange_ShouldPersistAllowedGroups() =>
        WithSubmissionSlotFixtureAsync(async (scenario, context) =>
        {
            // Arrange
            var newGroupId = await scenario.CreateGroupAsync(context.AdminSession, $"New Group {context.Suffix}");
            context.AddCleanup(ApiScenario.DeleteGroup(newGroupId));

            // Act
            var response = await context.AdminSession.PatchAsync(
                $"/api/submission-slots/{context.SlotId}",
                new
                {
                    allowedGroupIds = new[] { newGroupId }
                });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var updatedSlot = await scenario.GetSlotAsync(context.AdminSession, context.SlotId);
            updatedSlot.AllowedGroups.Should().NotBeNull();
            updatedSlot.AllowedGroups!.Select(group => group.Id).Should().BeEquivalentTo([newGroupId]);
        });

    /// <summary>
    /// Verifies that <c>AddAdmission</c> succeeds when the caller is the assigned teacher.
    /// </summary>
    [Fact]
    public Task AddAdmission_WhenUserIsAssignedTeacher_ShouldSucceed() =>
        WithSubmissionSlotFixtureAsync(async (_, context) =>
        {
            // Act
            var response = await context.TeacherSession.PostAsync<object?>(
                $"/api/submission-slots/{context.SlotId}/admissions/{context.StudentId}", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var admissionId = await context.TeacherSession.ReadAsync<Guid>(response);
            admissionId.Should().NotBeEmpty();
        });

    /// <summary>
    /// Verifies that <c>AddAdmission</c> returns <c>Forbidden</c> when the caller is another teacher.
    /// </summary>
    [Fact]
    public Task AddAdmission_WhenUserIsAnotherTeacher_ShouldReturnForbidden() =>
        WithSubmissionSlotFixtureAsync(async (scenario, context) =>
        {
            // Arrange
            var foreignTeacherId = await scenario.CreateTeacherAsync(
                context.AdminSession,
                $"teacher_foreign_{context.Suffix}",
                $"teacher_foreign_{context.Suffix}@example.com",
                "Pavel",
                "Sidorov");

            context.AddCleanup(ApiScenario.DeleteTeacher(foreignTeacherId));

            var foreignTeacherSession = await scenario.LoginAsync($"teacher_foreign_{context.Suffix}", "Teacher123!");

            using (foreignTeacherSession)
            {
                // Act
                var response = await foreignTeacherSession.PostAsync<object?>(
                    $"/api/submission-slots/{context.SlotId}/admissions/{context.StudentId}", null);

                // Assert
                await foreignTeacherSession.AssertProblemAsync(response, HttpStatusCode.Forbidden, "Forbidden", "own slots");
            }
        });

    /// <summary>
    /// Verifies that <c>UpdateNotificationSettings</c> succeeds when a student targets themselves.
    /// </summary>
    [Fact]
    public Task UpdateNotificationSettings_WhenStudentTargetsSelf_ShouldSucceed() =>
        WithSubmissionSlotFixtureAsync(async (_, context) =>
        {
            // Act
            var response = await context.StudentSession.PutAsync(
                $"/api/submission-slots/{context.SlotId}/notification-settings",
                new
                {
                    userId = context.StudentId,
                    submissionRemindTimes = new[] { SubmissionRemindTime.TwoDays, SubmissionRemindTime.FourHours },
                    bookingNotificationMode = BookingNotificationMode.OnlyNewBooking
                });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        });

    /// <summary>
    /// Verifies that <c>UpdateNotificationSettings</c> returns <c>Forbidden</c> when a student targets another user.
    /// </summary>
    [Fact]
    public Task UpdateNotificationSettings_WhenStudentTargetsAnotherUser_ShouldReturnForbidden() =>
        WithSubmissionSlotFixtureAsync(async (_, context) =>
        {
            // Act
            var response = await context.StudentSession.PutAsync(
                $"/api/submission-slots/{context.SlotId}/notification-settings",
                new
                {
                    userId = Guid.NewGuid(),
                    submissionRemindTimes = new[] { SubmissionRemindTime.OneWeek },
                    bookingNotificationMode = BookingNotificationMode.All
                });

            // Assert
            await context.StudentSession.AssertProblemAsync(response, HttpStatusCode.Forbidden, "Forbidden", "own notification settings");
        });

    /// <summary>
    /// Verifies that <c>UpdateNotificationSettings</c> returns a validation error when reminder times contain duplicates.
    /// </summary>
    [Fact]
    public Task UpdateNotificationSettings_WhenReminderTimesContainDuplicates_ShouldReturnValidationError() =>
        WithSubmissionSlotFixtureAsync(async (_, context) =>
        {
            // Act
            var response = await context.AdminSession.PutAsync(
                $"/api/submission-slots/{context.SlotId}/notification-settings",
                new
                {
                    userId = context.StudentId,
                    submissionRemindTimes = new[] { SubmissionRemindTime.OneWeek, SubmissionRemindTime.OneWeek },
                    bookingNotificationMode = BookingNotificationMode.All
                });

            // Assert
            await context.AdminSession.AssertProblemAsync(
                response, HttpStatusCode.UnprocessableEntity, "Validation Error", "Reminder schedules must not contain duplicates");
        });

    /// <summary>
    /// Verifies that <c>GetReminderSnapshot</c> returns the expected recipients when notification settings exist.
    /// </summary>
    [Fact]
    public Task GetReminderSnapshot_WhenNotificationSettingsExist_ShouldReturnExpectedRecipients() =>
        WithSubmissionSlotFixtureAsync(async (_, context) =>
        {
            // Arrange
            await context.StudentSession.PutAsync(
                $"/api/submission-slots/{context.SlotId}/notification-settings",
                new
                {
                    userId = context.StudentId,
                    submissionRemindTimes = new[] { SubmissionRemindTime.TwoDays, SubmissionRemindTime.FourHours },
                    bookingNotificationMode = BookingNotificationMode.OnlyNewBooking
                });

            // Act
            var response = await context.AdminSession.GetAsync("/api/submission-slots/reminder-snapshot");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var snapshot = (await context.AdminSession.ReadAsync<SubmissionSlotReminderSnapshotView[]>(response))!;
            var slotSnapshot = snapshot.Should().ContainSingle(x => x.SubmissionSlot.Id == context.SlotId).Subject;
            slotSnapshot.SubmissionSlot.Subject.Should().NotBeNull();
            slotSnapshot.SubmissionSlot.Subject!.Name.Should().Be(context.SubjectName);
            slotSnapshot.SubmissionSlot.Teacher.Should().NotBeNull();
            slotSnapshot.SubmissionSlot.Teacher!.LastName.Should().Be("Ivanova");
            slotSnapshot.SubmissionSlot.Teacher.FirstName.Should().Be("Anna");
            slotSnapshot.SubmissionSlot.AllowedGroups.Should().NotBeNull();
            slotSnapshot.SubmissionSlot.AllowedGroups!.Select(group => group.Name).Should().BeEquivalentTo(context.GroupName);

            slotSnapshot.Recipients.Should().BeEquivalentTo(
                [
                    new { UserId = context.StudentId, Email = context.StudentEmail, RemindTime = SubmissionRemindTimeModel.TwoDays },
                    new { UserId = context.StudentId, Email = context.StudentEmail, RemindTime = SubmissionRemindTimeModel.FourHours }
                ],
                options => options.ExcludingMissingMembers());
        });

    /// <summary>
    /// Verifies that <c>RemoveAdmission</c> succeeds when the caller is the assigned teacher.
    /// </summary>
    [Fact]
    public Task RemoveAdmission_WhenUserIsAssignedTeacher_ShouldSucceed() =>
        WithSubmissionSlotFixtureAsync(async (_, context) =>
        {
            // Arrange
            var addResponse = await context.TeacherSession.PostAsync<object?>(
                $"/api/submission-slots/{context.SlotId}/admissions/{context.StudentId}", null);

            addResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Act
            var removeResponse = await context.TeacherSession.DeleteAsync($"/api/submission-slots/{context.SlotId}/admissions/{context.StudentId}");

            // Assert
            removeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        });

    private async Task WithSubmissionSlotFixtureAsync(Func<ApiScenario, SubmissionSlotFixture, Task> assertion)
    {
        // Arrange
        var scenario = new ApiScenario(fixture);
        var (adminSession, _) = await scenario.CreateAdminSessionAsync();

        using (adminSession)
        {
            var context = await CreateSubmissionSlotFixtureAsync(scenario, adminSession);

            using (context.TeacherSession)
            using (context.StudentSession)
            {
                try
                {
                    await assertion(scenario, context);
                }
                finally
                {
                    await scenario.CleanupAsync(adminSession, context.CleanupActions.ToArray());
                }
            }
        }
    }

    private static async Task<SubmissionSlotFixture> CreateSubmissionSlotFixtureAsync(ApiScenario scenario, ApiSession adminSession)
    {
        var groupName = $"Group {scenario.Suffix}";
        var subjectName = $"Subject {scenario.Suffix}";
        var groupId = await scenario.CreateGroupAsync(adminSession, groupName);
        var teacherUsername = $"teacher_{scenario.Suffix}";

        var teacherId = await scenario.CreateTeacherAsync(
            adminSession,
            teacherUsername,
            $"{teacherUsername}@example.com",
            "Anna",
            "Ivanova");

        var studentUsername = $"student_{scenario.Suffix}";
        var studentEmail = $"{studentUsername}@example.com";

        var studentId = await scenario.CreateStudentAsync(
            adminSession,
            groupId,
            studentUsername,
            studentEmail,
            "Roman",
            "Smirnov");

        var subjectId = await scenario.CreateSubjectAsync(adminSession, teacherId, groupId, subjectName);
        var slotStart = DateTime.UtcNow.AddDays(8).Date.AddHours(14);
        var slotId = await scenario.CreateSlotAsync(adminSession, subjectId, teacherId, false, [groupId], slotStart);
        var teacherSession = await scenario.LoginAsync(teacherUsername, "Teacher123!");
        var studentSession = await scenario.LoginAsync(studentUsername, "Student123!");

        return new SubmissionSlotFixture(
            adminSession,
            teacherSession,
            studentSession,
            scenario.Suffix,
            groupId,
            groupName,
            teacherId,
            studentId,
            studentEmail,
            subjectId,
            subjectName,
            slotId,
            slotStart);
    }
}
