using System.Net;
using EFlow.Booking.ApiTests.Catalog.Support;
using EFlow.Booking.ApiTests.Infrastructure.Assertions;
using EFlow.Booking.ApiTests.Infrastructure.Collections;
using EFlow.Booking.ApiTests.Infrastructure.Fixtures;
using EFlow.Booking.ApiTests.Infrastructure.Scenarios;
using EFlow.Booking.ApiTests.Infrastructure.Sessions;
using EFlow.Booking.Contracts.Groups;
using EFlow.Booking.Contracts.Students;
using EFlow.Booking.Contracts.Subjects;
using EFlow.Booking.Contracts.Teachers;
using FluentAssertions;

namespace EFlow.Booking.ApiTests.Catalog;

/// <summary>
/// Verifies catalog endpoints for projections and validation rules.
/// </summary>
[Collection(ApiTestCollection.Name)]
public sealed class CatalogApiTests(ApiTestStackFixture fixture)
{
    /// <summary>
    /// Verifies that <c>GetGroups</c> returns the created group when a group exists.
    /// </summary>
    [Fact]
    public Task GetGroups_WhenGroupExists_ShouldReturnCreatedGroup() =>
        WithCatalogFixtureAsync(async (scenario, context) =>
        {
            // Arrange
            var group = await scenario.GetGroupAsync(context.AdminSession, context.GroupId);

            // Act
            var response = await context.AdminSession.GetAsync("/api/groups");

            // Assert
            group.Id.Should().Be(context.GroupId);
            group.Name.Should().Be(context.GroupName);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var groups = (await context.AdminSession.ReadAsync<GroupView[]>(response))!;
            groups.Should().Contain(x => x.Id == context.GroupId && x.Name == context.GroupName);
        });

    /// <summary>
    /// Verifies that <c>GetTeachers</c> returns the created teacher when a teacher exists.
    /// </summary>
    [Fact]
    public Task GetTeachers_WhenTeacherExists_ShouldReturnCreatedTeacher() =>
        WithCatalogFixtureAsync(async (scenario, context) =>
        {
            // Arrange
            var teacher = await scenario.GetTeacherAsync(context.AdminSession, context.TeacherId);

            // Act
            var response = await context.AdminSession.GetAsync("/api/teachers");

            // Assert
            teacher.Id.Should().Be(context.TeacherId);
            teacher.FirstName.Should().Be("Ivan");
            teacher.LastName.Should().Be("Petrov");
            teacher.MiddleName.Should().Be("M");
            teacher.BirthDate.Should().Be(new DateOnly(1990, 01, 01));
            teacher.CreatedAt.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var teachers = (await context.AdminSession.ReadAsync<TeacherView[]>(response))!;
            teachers.Should().Contain(x => x.Id == context.TeacherId && x.FirstName == "Ivan" && x.LastName == "Petrov");
        });

    /// <summary>
    /// Verifies that <c>GetStudents</c> returns the created student when a student exists.
    /// </summary>
    [Fact]
    public Task GetStudents_WhenStudentExists_ShouldReturnCreatedStudent() =>
        WithCatalogFixtureAsync(async (scenario, context) =>
        {
            // Arrange
            var student = await scenario.GetStudentAsync(context.AdminSession, context.StudentId);

            // Act
            var response = await context.AdminSession.GetAsync("/api/students");

            // Assert
            student.Id.Should().Be(context.StudentId);
            student.Group.Should().NotBeNull();
            student.Group!.Id.Should().Be(context.GroupId);
            student.FirstName.Should().Be("Petr");
            student.LastName.Should().Be("Sidorov");
            student.MiddleName.Should().Be("M");
            student.BirthDate.Should().Be(new DateOnly(2004, 01, 01));
            student.CreatedAt.Should().BeAfter(DateTime.UtcNow.AddMinutes(-10));
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var students = (await context.AdminSession.ReadAsync<StudentView[]>(response))!;
            students.Should().Contain(x => x.Id == context.StudentId && x.Group != null && x.Group.Id == context.GroupId);
        });

    /// <summary>
    /// Verifies that <c>GetSubjects</c> returns the created subject when a subject exists.
    /// </summary>
    [Fact]
    public Task GetSubjects_WhenSubjectExists_ShouldReturnCreatedSubject() =>
        WithCatalogFixtureAsync(async (scenario, context) =>
        {
            // Arrange
            var subject = await scenario.GetSubjectAsync(context.AdminSession, context.SubjectId);

            // Act
            var allResponse = await context.AdminSession.GetAsync("/api/subjects");
            var response = await context.AdminSession.GetAsync($"/api/subjects/by-teacher/{context.TeacherId}");

            // Assert
            subject.Id.Should().Be(context.SubjectId);
            subject.Name.Should().Be(context.SubjectName);
            subject.Teacher.Should().NotBeNull();
            subject.Teacher!.Id.Should().Be(context.TeacherId);
            subject.Groups.Should().NotBeNull();
            subject.Groups!.Select(x => x.Id).Should().BeEquivalentTo([context.GroupId]);
            allResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var allSubjects = (await context.AdminSession.ReadAsync<SubjectView[]>(allResponse))!;
            var subjects = (await context.AdminSession.ReadAsync<SubjectView[]>(response))!;
            allSubjects.Should().Contain(x =>
                x.Id == context.SubjectId &&
                x.Teacher != null &&
                x.Teacher.Id == context.TeacherId &&
                x.Groups != null &&
                x.Groups.Any(g => g.Id == context.GroupId));
            subjects.Should().Contain(x => x.Id == context.SubjectId && x.Teacher != null && x.Teacher.Id == context.TeacherId);
        });

    /// <summary>
    /// Verifies that <c>CreateGroup</c> returns a validation error when the name is empty.
    /// </summary>
    [Fact]
    public Task CreateGroup_WhenNameIsEmpty_ShouldReturnValidationError() =>
        WithCatalogFixtureAsync(async (_, context) =>
        {
            // Act
            var response = await context.AdminSession.PostAsync("/api/groups", new { Name = string.Empty });

            // Assert
            await context.AdminSession.AssertProblemAsync(response, HttpStatusCode.UnprocessableEntity, "Validation Error", "Group name is required");
        });

    /// <summary>
    /// Verifies that <c>CreateTeacher</c> returns validation details when the payload is invalid.
    /// </summary>
    [Fact]
    public Task CreateTeacher_WhenPayloadIsInvalid_ShouldReturnValidationError() =>
        WithCatalogFixtureAsync(async (_, context) =>
        {
            // Act
            var response = await context.AdminSession.PostAsync(
                "/api/teachers",
                new
                {
                    userName = $"bad_teacher_{context.Suffix}",
                    password = "123",
                    email = "not-an-email",
                    firstName = string.Empty,
                    middleName = "M",
                    lastName = string.Empty,
                    birthDate = "2012-01-01"
                });

            // Assert
            await context.AdminSession.AssertProblemAsync(response, HttpStatusCode.UnprocessableEntity, "Validation Error");
            var errorText = await context.AdminSession.ReadTextAsync(response);
            errorText.Should().Contain("Email must be valid");
            errorText.Should().Contain("Teacher must be at least 18 years old");
        });

    private async Task WithCatalogFixtureAsync(Func<ApiScenario, CatalogFixture, Task> assertion)
    {
        var scenario = new ApiScenario(fixture);
        var (adminSession, _) = await scenario.CreateAdminSessionAsync();

        using (adminSession)
        {
            var context = await CreateCatalogFixtureAsync(scenario, adminSession);

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

    private static async Task<CatalogFixture> CreateCatalogFixtureAsync(ApiScenario scenario, ApiSession adminSession)
    {
        var groupName = $"Group {scenario.Suffix}";
        var subjectName = $"Subject {scenario.Suffix}";

        var groupId = await scenario.CreateGroupAsync(adminSession, groupName);

        var teacherId = await scenario.CreateTeacherAsync(
            adminSession,
            $"teacher_{scenario.Suffix}",
            $"teacher_{scenario.Suffix}@example.com",
            "Ivan",
            "Petrov");

        var studentId = await scenario.CreateStudentAsync(
            adminSession,
            groupId,
            $"student_{scenario.Suffix}",
            $"student_{scenario.Suffix}@example.com",
            "Petr",
            "Sidorov");

        var subjectId = await scenario.CreateSubjectAsync(adminSession, teacherId, groupId, subjectName);

        return new CatalogFixture(adminSession, scenario.Suffix, groupId, groupName, teacherId, studentId, subjectId, subjectName);
    }
}
