using System.Net;
using EFlow.Booking.ApiTests.Bookings.Support;
using EFlow.Booking.ApiTests.Infrastructure.Assertions;
using EFlow.Booking.ApiTests.Infrastructure.Collections;
using EFlow.Booking.ApiTests.Infrastructure.Fixtures;
using EFlow.Booking.ApiTests.Infrastructure.Scenarios;
using EFlow.Booking.ApiTests.Infrastructure.Sessions;
using EFlow.Booking.Contracts.BookingRecords;
using EFlow.Booking.WebApi.Contracts.Bookings;
using FluentAssertions;

namespace EFlow.Booking.ApiTests.Bookings;

/// <summary>
/// Verifies booking endpoints for reads and authorization rules.
/// </summary>
[Collection(ApiTestCollection.Name)]
public sealed class BookingsApiTests(ApiTestStackFixture fixture)
{
    /// <summary>
    /// Verifies that <c>GetBookingById</c> returns the expected booking when the booking exists.
    /// </summary>
    [Fact]
    public Task GetBookingById_WhenBookingExists_ShouldReturnExpectedBooking() =>
        WithBookingsFixtureAsync(async (scenario, context) =>
        {
            // Arrange
            var bookingId = await scenario.CreateBookingAsync(context.AdminSession, context.Student1Id, context.SlotId);
            context.AddCleanup(ApiScenario.DeleteBooking(bookingId));

            // Act
            var booking = await scenario.GetBookingAsync(context.AdminSession, bookingId);

            // Assert
            booking.Id.Should().Be(bookingId);
            booking.Student.Should().NotBeNull();
            booking.Student!.Id.Should().Be(context.Student1Id);
            booking.Slot.Should().NotBeNull();
            booking.Slot!.Id.Should().Be(context.SlotId);
            booking.CreatedAt.Should().BeAfter(DateTime.UtcNow.AddMinutes(-10));
        });

    /// <summary>
    /// Verifies that <c>GetAllBookings</c> returns all created bookings when the caller is an administrator.
    /// </summary>
    [Fact]
    public Task GetAllBookings_WhenUserIsAdmin_ShouldReturnAllCreatedBookings() =>
        WithTwoBookingsAsync(async (_, context) =>
        {
            // Act
            var response = await context.AdminSession.GetAsync(ApiScenario.BookingsPath);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var bookings = (await context.AdminSession.ReadAsync<BookingRecordView[]>(response))!;
            bookings.Should().Contain(x => x.Id == context.Booking1Id && x.Student != null && x.Student.Id == context.Student1Id);
            bookings.Should().Contain(x => x.Id == context.Booking2Id && x.Student != null && x.Student.Id == context.Student2Id);
        });

    /// <summary>
    /// Verifies that <c>GetBookingsByStudent</c> returns only the caller's bookings when the caller requests their own student identifier.
    /// </summary>
    [Fact]
    public Task GetBookingsByStudent_WhenUserRequestsOwnBookings_ShouldReturnOwnBookings() =>
        WithTwoBookingsAsync(async (_, context) =>
        {
            // Act
            var response = await context.Student1Session.GetAsync($"/api/bookings/by-student/{context.Student1Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var bookings = (await context.Student1Session.ReadAsync<BookingRecordView[]>(response))!;
            bookings.Should().ContainSingle(x => x.Id == context.Booking1Id);
        });

    /// <summary>
    /// Verifies that <c>GetBookingsByStudent</c> returns <c>Forbidden</c> when a student requests another student's bookings.
    /// </summary>
    [Fact]
    public Task GetBookingsByStudent_WhenUserRequestsAnotherStudentsBookings_ShouldReturnForbidden() =>
        WithTwoBookingsAsync(async (_, context) =>
        {
            // Act
            var response = await context.Student1Session.GetAsync($"/api/bookings/by-student/{context.Student2Id}");

            // Assert
            await context.Student1Session.AssertProblemAsync(response, HttpStatusCode.Forbidden, ApiAssertions.ForbiddenTitle, "view your own bookings");
        });

    /// <summary>
    /// Verifies that <c>CreateBooking</c> returns <c>Forbidden</c> when a student targets another student.
    /// </summary>
    [Fact]
    public Task CreateBooking_WhenStudentTargetsAnotherStudent_ShouldReturnForbidden() =>
        WithBookingsFixtureAsync(async (_, context) =>
        {
            // Act
            var response = await context.Student1Session.PostAsync(
                ApiScenario.BookingsPath,
                new CreateBookingRequest
                {
                    StudentId = context.Student2Id,
                    SlotId = context.SlotId
                });

            // Assert
            await context.Student1Session.AssertProblemAsync(response, HttpStatusCode.Forbidden, ApiAssertions.ForbiddenTitle, "create bookings for yourself");
        });

    /// <summary>
    /// Verifies that <c>DeleteBooking</c> returns <c>Forbidden</c> when a student targets another student's booking.
    /// </summary>
    [Fact]
    public Task DeleteBooking_WhenStudentTargetsAnotherStudentsBooking_ShouldReturnForbidden() =>
        WithTwoBookingsAsync(async (_, context) =>
        {
            // Act
            var response = await context.Student1Session.DeleteAsync($"/api/bookings/{context.Booking2Id}");

            // Assert
            await context.Student1Session.AssertProblemAsync(response, HttpStatusCode.Forbidden, ApiAssertions.ForbiddenTitle, "delete your own bookings");
        });

    /// <summary>
    /// Verifies that <c>GetBookingsBySlot</c> returns all bookings for the slot when the caller is an administrator.
    /// </summary>
    [Fact]
    public Task GetBookingsBySlot_WhenUserIsAdmin_ShouldReturnBookingsForSlot() =>
        WithTwoBookingsAsync(async (_, context) =>
        {
            // Act
            var response = await context.AdminSession.GetAsync($"/api/bookings/by-slot/{context.SlotId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var bookings = (await context.AdminSession.ReadAsync<BookingRecordView[]>(response))!;
            bookings.Should().Contain(x => x.Id == context.Booking1Id);
            bookings.Should().Contain(x => x.Id == context.Booking2Id);
        });

    private async Task WithBookingsFixtureAsync(Func<ApiScenario, BookingsFixture, Task> assertion)
    {
        // Arrange
        var scenario = new ApiScenario(fixture);
        var (adminSession, _) = await scenario.CreateAdminSessionAsync();

        using (adminSession)
        {
            var context = await CreateBookingsFixtureAsync(scenario, adminSession);

            using (context.Student1Session)
            {
                try
                {
                    await assertion(scenario, context);
                }
                finally
                {
                    await scenario.CleanupAsync(
                        adminSession,
                        context.CleanupActions.ToArray());
                }
            }
        }
    }

    private Task WithTwoBookingsAsync(Func<ApiScenario, BookingsFixture, Task> assertion) =>
        WithBookingsFixtureAsync(async (scenario, context) =>
        {
            context.Booking1Id = await scenario.CreateBookingAsync(context.AdminSession, context.Student1Id, context.SlotId);
            context.Booking2Id = await scenario.CreateBookingAsync(context.AdminSession, context.Student2Id, context.SlotId);
            context.AddCleanup(ApiScenario.DeleteBooking(context.Booking1Id));
            context.AddCleanup(ApiScenario.DeleteBooking(context.Booking2Id));
            
            await assertion(scenario, context);
        });

    private static async Task<BookingsFixture> CreateBookingsFixtureAsync(ApiScenario scenario, ApiSession adminSession)
    {
        var groupId = await scenario.CreateGroupAsync(adminSession, $"Group {scenario.Suffix}");
        var teacherUsername = $"teacher_{scenario.Suffix}";
        
        var teacherId = await scenario.CreateTeacherAsync(
            adminSession,
            teacherUsername,
            $"{teacherUsername}@example.com",
            "Olga",
            "Petrova");

        var student1Username = $"student1_{scenario.Suffix}";
        var student2Username = $"student2_{scenario.Suffix}";

        var student1Id = await scenario.CreateStudentAsync(
            adminSession,
            groupId,
            student1Username,
            $"{student1Username}@example.com",
            "Petr",
            "Ivanov");

        var student2Id = await scenario.CreateStudentAsync(
            adminSession,
            groupId,
            student2Username,
            $"{student2Username}@example.com",
            "Sergey",
            "Smirnov");

        var subjectId = await scenario.CreateSubjectAsync(adminSession, teacherId, groupId, $"Subject {scenario.Suffix}");
        var slotId = await scenario.CreateSlotAsync(adminSession, subjectId, teacherId, true);
        await scenario.AddAdmissionAsync(adminSession, slotId, student1Id);
        await scenario.AddAdmissionAsync(adminSession, slotId, student2Id);
        var student1Session = await scenario.LoginAsync(student1Username, ApiScenario.StudentPassword);
        
        return new BookingsFixture(adminSession, student1Session, groupId, teacherId, subjectId, slotId, student1Id, student2Id);
    }
}
