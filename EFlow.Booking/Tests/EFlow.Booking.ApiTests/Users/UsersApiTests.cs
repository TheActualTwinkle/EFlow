using System.Net;
using EFlow.Booking.ApiTests.Infrastructure.Assertions;
using EFlow.Booking.ApiTests.Infrastructure.Collections;
using EFlow.Booking.ApiTests.Infrastructure.Contracts;
using EFlow.Booking.ApiTests.Infrastructure.Fixtures;
using EFlow.Booking.ApiTests.Infrastructure.Scenarios;
using EFlow.Booking.ApiTests.Infrastructure.Sessions;
using FluentAssertions;

namespace EFlow.Booking.ApiTests.Users;

/// <summary>
/// Verifies user account management endpoints.
/// </summary>
[Collection(ApiTestCollection.Name)]
public sealed class UsersApiTests(ApiTestStackFixture fixture)
{
    /// <summary>
    /// Verifies that a teacher can update their own email address.
    /// </summary>
    [Fact]
    public Task UpdateEmail_WhenTeacherUpdatesOwnAccount_ShouldUpdateCurrentUserEmail() =>
        WithUsersFixtureAsync(async (scenario, context) =>
        {
            // Arrange
            var newEmail = $"teacher_new_{context.Suffix}@example.com";

            // Act
            var response = await context.TeacherSession.PatchAsync(
                $"/api/users/{context.TeacherId}/email",
                new UpdateUserEmailRequestModel
                {
                    Email = newEmail
                });
            var meResponse = await context.TeacherSession.GetAsync(ApiScenario.CurrentUserPath);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            meResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var currentUser = await context.TeacherSession.ReadAsync<CurrentUserResponse>(meResponse);
            currentUser!.Email.Should().Be(newEmail);
        });

    /// <summary>
    /// Verifies that a student cannot update another user's email address.
    /// </summary>
    [Fact]
    public Task UpdateEmail_WhenStudentTargetsAnotherUser_ShouldReturnForbidden() =>
        WithUsersFixtureAsync(async (_, context) =>
        {
            // Act
            var response = await context.StudentSession.PatchAsync(
                $"/api/users/{context.TeacherId}/email",
                new UpdateUserEmailRequestModel
                {
                    Email = $"forbidden_{context.Suffix}@example.com"
                });

            // Assert
            await context.StudentSession.AssertProblemAsync(response, HttpStatusCode.Forbidden, ApiAssertions.ForbiddenTitle, "own account");
        });

    /// <summary>
    /// Verifies that an administrator can update another user's email address.
    /// </summary>
    [Fact]
    public Task UpdateEmail_WhenAdminTargetsAnotherUser_ShouldUpdateEmail() =>
        WithUsersFixtureAsync(async (scenario, context) =>
        {
            // Arrange
            var newEmail = $"student_admin_new_{context.Suffix}@example.com";

            // Act
            var response = await context.AdminSession.PatchAsync(
                $"/api/users/{context.StudentId}/email",
                new UpdateUserEmailRequestModel
                {
                    Email = newEmail
                });

            using var studentSession = await scenario.LoginAsync(context.StudentUsername, ApiScenario.StudentPassword);
            var meResponse = await studentSession.GetAsync(ApiScenario.CurrentUserPath);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            meResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var currentUser = await studentSession.ReadAsync<CurrentUserResponse>(meResponse);
            currentUser!.Email.Should().Be(newEmail);
        });

    /// <summary>
    /// Verifies that a student can change their own password and use it for subsequent login.
    /// </summary>
    [Fact]
    public Task UpdatePassword_WhenStudentProvidesCurrentPassword_ShouldChangePassword() =>
        WithUsersFixtureAsync(async (scenario, context) =>
        {
            // Arrange
            var newPassword = $"NewStudent{context.Suffix}!";

            // Act
            var response = await context.StudentSession.PatchAsync(
                $"/api/users/{context.StudentId}/password",
                new UpdateUserPasswordRequestModel
                {
                    CurrentPassword = ApiScenario.StudentPassword,
                    NewPassword = newPassword
                });

            using var oldPasswordSession = fixture.CreateSession();
            var oldLoginResponse = await oldPasswordSession.PostAsync(
                ApiScenario.LoginPath,
                new LoginRequestModel
                {
                    Username = context.StudentUsername,
                    Password = ApiScenario.StudentPassword
                });
            using var newPasswordSession = await scenario.LoginAsync(context.StudentUsername, newPassword);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            oldLoginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            newPasswordSession.Should().NotBeNull();
        });

    /// <summary>
    /// Verifies that a password change with an invalid current password returns validation problem details.
    /// </summary>
    [Fact]
    public Task UpdatePassword_WhenCurrentPasswordIsInvalid_ShouldReturnPasswordMismatch() =>
        WithUsersFixtureAsync(async (_, context) =>
        {
            // Act
            var response = await context.StudentSession.PatchAsync(
                $"/api/users/{context.StudentId}/password",
                new UpdateUserPasswordRequestModel
                {
                    CurrentPassword = "WrongPassword!",
                    NewPassword = $"NewStudent{context.Suffix}!"
                });

            // Assert
            await context.StudentSession.AssertValidationProblemAsync(response, HttpStatusCode.BadRequest, "PasswordMismatch", "Incorrect password");
        });

    /// <summary>
    /// Verifies that an administrator must provide their own current password before resetting another user's password.
    /// </summary>
    [Fact]
    public Task UpdatePassword_WhenAdminPasswordIsInvalid_ShouldReturnCurrentPasswordValidationError() =>
        WithUsersFixtureAsync(async (_, context) =>
        {
            // Act
            var response = await context.AdminSession.PatchAsync(
                $"/api/users/{context.TeacherId}/password",
                new UpdateUserPasswordRequestModel
                {
                    CurrentPassword = "WrongAdminPassword!",
                    NewPassword = $"NewTeacher{context.Suffix}!"
                });

            // Assert
            await context.AdminSession.AssertValidationProblemAsync(response, HttpStatusCode.BadRequest, "CurrentPassword", "invalid");
        });

    /// <summary>
    /// Verifies that an administrator can reset another user's password after confirming the administrator password.
    /// </summary>
    [Fact]
    public Task UpdatePassword_WhenAdminProvidesCurrentPassword_ShouldResetTargetUserPassword() =>
        WithUsersFixtureAsync(async (scenario, context) =>
        {
            // Arrange
            var newPassword = $"NewTeacher{context.Suffix}!";

            // Act
            var response = await context.AdminSession.PatchAsync(
                $"/api/users/{context.TeacherId}/password",
                new UpdateUserPasswordRequestModel
                {
                    CurrentPassword = fixture.AdminPassword,
                    NewPassword = newPassword
                });

            using var oldPasswordSession = fixture.CreateSession();
            var oldLoginResponse = await oldPasswordSession.PostAsync(
                ApiScenario.LoginPath,
                new LoginRequestModel
                {
                    Username = context.TeacherUsername,
                    Password = ApiScenario.TeacherPassword
                });
            using var newPasswordSession = await scenario.LoginAsync(context.TeacherUsername, newPassword);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            oldLoginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            newPasswordSession.Should().NotBeNull();
        });

    private async Task WithUsersFixtureAsync(Func<ApiScenario, UsersFixture, Task> assertion)
    {
        var scenario = new ApiScenario(fixture);
        var (adminSession, _) = await scenario.CreateAdminSessionAsync();

        using (adminSession)
        {
            var context = await CreateUsersFixtureAsync(scenario, adminSession);

            try
            {
                await assertion(scenario, context);
            }
            finally
            {
                context.TeacherSession.Dispose();
                context.StudentSession.Dispose();
                await scenario.CleanupAsync(adminSession, context.CleanupActions.ToArray());
            }
        }
    }

    private static async Task<UsersFixture> CreateUsersFixtureAsync(ApiScenario scenario, ApiSession adminSession)
    {
        var groupId = await scenario.CreateGroupAsync(adminSession, $"Users Group {scenario.Suffix}");
        var teacherUsername = $"users_teacher_{scenario.Suffix}";
        var studentUsername = $"users_student_{scenario.Suffix}";

        var teacherId = await scenario.CreateTeacherAsync(
            adminSession,
            teacherUsername,
            $"users_teacher_{scenario.Suffix}@example.com",
            "Ivan",
            "Petrov");

        var studentId = await scenario.CreateStudentAsync(
            adminSession,
            groupId,
            studentUsername,
            $"users_student_{scenario.Suffix}@example.com",
            "Petr",
            "Sidorov");

        var teacherSession = await scenario.LoginAsync(teacherUsername, ApiScenario.TeacherPassword);
        var studentSession = await scenario.LoginAsync(studentUsername, ApiScenario.StudentPassword);

        return new UsersFixture(
            adminSession,
            teacherSession,
            studentSession,
            scenario.Suffix,
            groupId,
            teacherId,
            teacherUsername,
            studentId,
            studentUsername);
    }

    private sealed record UsersFixture(
        ApiSession AdminSession,
        ApiSession TeacherSession,
        ApiSession StudentSession,
        string Suffix,
        Guid GroupId,
        Guid TeacherId,
        string TeacherUsername,
        Guid StudentId,
        string StudentUsername)
    {
        public IEnumerable<Func<ApiSession, Task>> CleanupActions =>
        [
            ApiScenario.DeleteStudent(StudentId),
            ApiScenario.DeleteTeacher(TeacherId),
            ApiScenario.DeleteGroup(GroupId)
        ];
    }
}
