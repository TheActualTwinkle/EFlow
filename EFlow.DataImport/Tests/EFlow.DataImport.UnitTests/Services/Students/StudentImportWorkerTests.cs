using System.Net;
using System.Text;
using System.Text.Json;
using EFlow.DataImport.Application.Models.Students;
using EFlow.DataImport.Application.Services.Students;
using EFlow.DataImport.Messaging.Booking.Abstractions;
using EFlow.DataImport.Messaging.Booking.Models;
using FluentAssertions;
using Moq;

namespace EFlow.DataImport.UnitTests.Services.Students;

public sealed class StudentImportWorkerTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task ImportStudentsAsync_WhenCsvIsValid_ShouldSendNormalizedStudentsToBookingAndReturnCombinedResult()
    {
        var groupId = Guid.NewGuid();
        IReadOnlyList<BookingImportedStudent>? importedStudents = null;
        var bookingClient = new Mock<IBookingStudentImportClient>();

        bookingClient
            .Setup(x => x.ImportStudentsAsync(groupId, It.IsAny<IReadOnlyList<BookingImportedStudent>>(), CancellationToken.None))
            .Callback<Guid, IReadOnlyList<BookingImportedStudent>, CancellationToken>((_, students, _) => importedStudents = students)
            .ReturnsAsync(
                new BookingStudentImportResult
                {
                    StatusCode = HttpStatusCode.OK,
                    ContentType = "application/json",
                    Body = JsonSerializer.Serialize(
                        new StudentsImportResult
                        {
                            TotalCount = 2,
                            ImportedCount = 2,
                            FailedCount = 0,
                            Errors = []
                        },
                        JsonOptions)
                });

        var worker = new StudentImportWorker(bookingClient.Object);

        var result = await worker.ImportStudentsAsync(
            CreateRequest(
                groupId,
                """
                Иванов; Иван; Иванович; <ivan@example.com>; <ivan.login@example.com>; Password123!; 04.06.2006
                Петров; Петр; ; petr@example.com; petr@example.com; Password456!; 05.07.2005
                """),
            CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.ContentType.Should().Be("application/json");
        var payload = JsonSerializer.Deserialize<StudentsImportResult>(result.Body!, JsonOptions);
        payload.Should().NotBeNull();
        payload.TotalCount.Should().Be(2);
        payload.ImportedCount.Should().Be(2);
        payload.FailedCount.Should().Be(0);
        payload.Errors.Should().BeEmpty();

        importedStudents.Should().NotBeNull();
        importedStudents.Should().HaveCount(2);
        importedStudents![0].Email.Should().Be("ivan@example.com");
        importedStudents[0].UserName.Should().Be("ivan.login@example.com");
        importedStudents[0].MiddleName.Should().Be("Иванович");
        importedStudents[1].MiddleName.Should().BeNull();
        importedStudents[1].BirthDate.Should().Be(new DateOnly(2005, 07, 05));
    }

    [Fact]
    public async Task ImportStudentsAsync_WhenMappingContainsUnknownField_ShouldReturnBadRequestAndNotCallBooking()
    {
        var bookingClient = new Mock<IBookingStudentImportClient>();
        var worker = new StudentImportWorker(bookingClient.Object);

        var result = await worker.ImportStudentsAsync(
            CreateRequest(
                Guid.NewGuid(),
                "Иванов; Иван; Иванович; ivan@example.com; ivan; Password123!; 04.06.2006",
                ["LastName", "FirstName", "MiddleName", "Email", "UserName", "Password", "Unknown"]),
            CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Body.Should().Be("Unknown column field 'Unknown'");

        bookingClient.Verify(
            x => x.ImportStudentsAsync(It.IsAny<Guid>(), It.IsAny<IReadOnlyList<BookingImportedStudent>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ImportStudentsAsync_WhenRowsContainParseErrors_ShouldImportValidRowsAndAggregateErrors()
    {
        var groupId = Guid.NewGuid();
        var bookingClient = new Mock<IBookingStudentImportClient>();

        bookingClient
            .Setup(x => x.ImportStudentsAsync(groupId, It.IsAny<IReadOnlyList<BookingImportedStudent>>(), CancellationToken.None))
            .ReturnsAsync(
                new BookingStudentImportResult
                {
                    StatusCode = HttpStatusCode.OK,
                    ContentType = "application/json",
                    Body = JsonSerializer.Serialize(
                        new StudentsImportResult
                        {
                            TotalCount = 1,
                            ImportedCount = 1,
                            FailedCount = 0,
                            Errors = []
                        },
                        JsonOptions)
                });

        var worker = new StudentImportWorker(bookingClient.Object);

        var result = await worker.ImportStudentsAsync(
            CreateRequest(
                groupId,
                """
                Иванов; Иван; Иванович; ivan@example.com; ivan@example.com; Password123!; 04.06.2006
                Петров; Петр; Петрович; petr@example.com; petr@example.com; Password456!; 2005-07-05
                """),
            CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = JsonSerializer.Deserialize<StudentsImportResult>(result.Body!, JsonOptions);
        payload.Should().NotBeNull();
        payload.TotalCount.Should().Be(2);
        payload.ImportedCount.Should().Be(1);
        payload.FailedCount.Should().Be(1);

        payload.Errors.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(
                new StudentImportRowError
                {
                    RowNumber = 2,
                    Message = "Invalid birth date. Expected format: dd.MM.yyyy"
                });
    }

    [Fact]
    public async Task ImportStudentsAsync_WhenBookingReturnsError_ShouldReturnBookingResponse()
    {
        var groupId = Guid.NewGuid();
        var bookingClient = new Mock<IBookingStudentImportClient>();

        bookingClient
            .Setup(x => x.ImportStudentsAsync(groupId, It.IsAny<IReadOnlyList<BookingImportedStudent>>(), CancellationToken.None))
            .ReturnsAsync(
                new BookingStudentImportResult
                {
                    StatusCode = HttpStatusCode.UnprocessableEntity,
                    Body = "Group not found",
                    ContentType = "text/plain"
                });

        var worker = new StudentImportWorker(bookingClient.Object);

        var result = await worker.ImportStudentsAsync(
            CreateRequest(groupId, "Иванов; Иван; Иванович; ivan@example.com; ivan@example.com; Password123!; 04.06.2006"),
            CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        result.Body.Should().Be("Group not found");
        result.ContentType.Should().Be("text/plain");
    }

    private static StudentImportProxyRequest CreateRequest(
        Guid groupId,
        string csv,
        IReadOnlyList<string>? fields = null,
        bool hasHeaderRow = false)
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        return new StudentImportProxyRequest
        {
            GroupId = groupId,
            FileStream = stream,
            FileName = "students.csv",
            ContentType = "text/csv",
            Fields = fields ??
            [
                "LastName",
                "FirstName",
                "MiddleName",
                "Email",
                "UserName",
                "Password",
                "BirthDate"
            ],
            HasHeaderRow = hasHeaderRow
        };
    }
}