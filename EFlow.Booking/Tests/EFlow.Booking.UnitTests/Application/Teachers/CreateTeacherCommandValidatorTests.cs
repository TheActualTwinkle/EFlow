using EFlow.Booking.Application.Teachers.Commands;
using EFlow.Common.Infrastructure;
using FluentAssertions;

namespace EFlow.Booking.UnitTests.Application.Teachers;

public class CreateTeacherCommandValidatorTests
{
    [Fact]
    public void Validate_WhenBirthDateIsExactlyAt18YearsBoundary_ShouldFail()
    {
        // Arrange
        var now = new DateTime(2026, 04, 18, 12, 0, 0, DateTimeKind.Utc);
        var systemClock = new FakeSystemClock(now);
        var validator = new CreateTeacherCommandValidator(systemClock);

        var command = new CreateTeacherCommand
        {
            UserName = "teacher.boundary",
            Password = "StrongPass1!",
            Email = "teacher@example.com",
            FirstName = "Ivan",
            LastName = "Petrov",
            BirthDate = DateOnly.FromDateTime(now.AddYears(-18))
        };

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x =>
            x.PropertyName == nameof(CreateTeacherCommand.BirthDate) &&
            x.ErrorMessage == "Teacher must be at least 18 years old");
    }
}
