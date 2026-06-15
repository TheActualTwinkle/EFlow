using System.Diagnostics;
using System.Net;
using EFlow.DataImport.ApiTests.Infrastructure.Collections;
using EFlow.DataImport.ApiTests.Infrastructure.Contracts;
using EFlow.DataImport.ApiTests.Infrastructure.Fixtures;
using EFlow.DataImport.ApiTests.Infrastructure.Sessions;
using FluentAssertions;

namespace EFlow.DataImport.ApiTests.Students;

[Collection(ApiTestCollection.Name)]
public sealed class StudentImportApiTests(DataImportApiTestStackFixture fixture)
{
    private static readonly string[] DefaultFields =
    [
        "LastName",
        "FirstName",
        "MiddleName",
        "Email",
        "UserName",
        "Password",
        "BirthDate"
    ];

    [Fact]
    public async Task ImportStudents_WhenAdminUploadsValidCsv_ShouldCreateStudentsInBooking()
    {
        using var session = fixture.CreateSession();
        await LoginAsAdminAsync(session);
        var groupId = await CreateGroupAsync(session);

        using var response = await session.DataImportPostMultipartAsync(
            groupId,
            """
            Иванов; Иван; Иванович; <student-import-ivan@example.com>; <student-import-ivan@example.com>; Student123!; 04.06.2006
            Петров; Петр; ; student-import-petr@example.com; student-import-petr@example.com; Student123!; 05.07.2005
            """,
            DefaultFields);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var importResult = await session.ReadAsync<StudentsImportResult>(response);
        importResult.Should().NotBeNull();
        importResult.TotalCount.Should().Be(2);
        importResult.ImportedCount.Should().Be(2);
        importResult.FailedCount.Should().Be(0);
        importResult.Errors.Should().BeEmpty();

        var students = await GetStudentsAsync(session);
        var importedStudents = students.Where(x => x.Group?.Id == groupId).ToArray();

        importedStudents.Should().ContainEquivalentOf(
            new StudentView
            {
                Id = Guid.Empty,
                FirstName = "Иван",
                LastName = "Иванов",
                MiddleName = "Иванович",
                BirthDate = new DateOnly(2006, 06, 04),
                Group = new GroupView { Id = groupId, Name = string.Empty }
            },
            options => options
                .Excluding(x => x.Id)
                .Excluding(x => x.Group!.Name));

        importedStudents.Should().ContainEquivalentOf(
            new StudentView
            {
                Id = Guid.Empty,
                FirstName = "Петр",
                LastName = "Петров",
                MiddleName = null,
                BirthDate = new DateOnly(2005, 07, 05),
                Group = new GroupView { Id = groupId, Name = string.Empty }
            },
            options => options
                .Excluding(x => x.Id)
                .Excluding(x => x.Group!.Name));
    }

    [Fact]
    public async Task ImportStudents_WhenCallerIsAnonymous_ShouldReturnUnauthorized()
    {
        using var session = fixture.CreateSession();

        using var response = await session.AnonymousDataImportPostMultipartAsync(
            Guid.NewGuid(),
            "Иванов; Иван; Иванович; ivan-anonymous@example.com; ivan-anonymous@example.com; Student123!; 04.06.2006",
            DefaultFields);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ImportStudents_WhenCsvContains50Students_ShouldCompleteWithin10Seconds()
    {
        using var session = fixture.CreateSession();
        await LoginAsAdminAsync(session);
        var groupId = await CreateGroupAsync(session);
        var csv = CreateStudentsCsv(50);

        var stopwatch = Stopwatch.StartNew();
        using var response = await session.DataImportPostMultipartAsync(groupId, csv, DefaultFields);
        stopwatch.Stop();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(10));

        var importResult = await session.ReadAsync<StudentsImportResult>(response);
        importResult.Should().NotBeNull();
        importResult.TotalCount.Should().Be(50);
        importResult.ImportedCount.Should().Be(50);
        importResult.FailedCount.Should().Be(0);
    }

    private async Task LoginAsAdminAsync(DataImportApiSession session)
    {
        using var response = await session.BookingPostAsync(
            "/api/auth/login",
            new LoginRequestModel
            {
                Username = fixture.AdminUsername,
                Password = fixture.AdminPassword
            });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static async Task<Guid> CreateGroupAsync(DataImportApiSession session)
    {
        using var response = await session.BookingPostAsync(
            "/api/groups",
            new CreateGroupRequest { Name = $"DataImport API {Guid.NewGuid():N}" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        return session.GetCreatedId(response);
    }

    private static async Task<IReadOnlyList<StudentView>> GetStudentsAsync(DataImportApiSession session)
    {
        using var response = await session.BookingGetAsync("/api/students");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return await session.ReadAsync<StudentView[]>(response) ?? [];
    }

    private static string CreateStudentsCsv(int count) =>
        string.Join(
            Environment.NewLine,
            Enumerable.Range(1, count).Select(index =>
            {
                var suffix = Guid.NewGuid().ToString("N")[..10];

                return
                    $"Фамилия{index}; Имя{index}; Отчество{index}; data-import-{suffix}@example.com; data-import-{suffix}@example.com; Student123!; 04.06.2006";
            }));
}