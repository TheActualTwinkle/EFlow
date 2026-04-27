using EFlow.Common.Extensions;
using FluentAssertions;

namespace EFlow.Common.UnitTests.Extensions;

public class TimeSpanExtensionsTests
{
    [Fact]
    public void ToCronExpression_WhenTimeSpanIsOneMinute_ShouldReturnMinuteCronExpression()
    {
        // Arrange
        var timeSpan = TimeSpan.FromMinutes(1);

        // Act
        var result = timeSpan.ToCronExpression();

        // Assert
        result.Should().Be("0 * * * * *");
    }

    [Fact]
    public void ToCronExpression_WhenTimeSpanIsOneHour_ShouldReturnHourlyCronExpression()
    {
        // Arrange
        var timeSpan = TimeSpan.FromHours(1);

        // Act
        var result = timeSpan.ToCronExpression();

        // Assert
        result.Should().Be("0 0 * * * *");
    }
    
    [Fact]
    public void ToCronExpression_WhenTimeSpanIsTenSeconds_ShouldReturnTenSecondsCronExpression()
    {
        // Arrange
        var timeSpan = TimeSpan.FromSeconds(10);

        // Act
        var result = timeSpan.ToCronExpression();

        // Assert
        result.Should().Be("0/10 * * * * *");
    }
    
    [Fact]
    public void ToCronExpression_WhenTimeSpanContainsDays_ShouldReturnDailyCronExpression()
    {
        // Arrange
        var timeSpan = new TimeSpan(2, 3, 4, 5);

        // Act
        var result = timeSpan.ToCronExpression();

        // Assert
        result.Should().Be("5 4 3 */2 * *");
    }

    [Fact]
    public void ToCronExpression_WhenTimeSpanContainsHours_ShouldReturnHourlyCronExpression()
    {
        // Arrange
        var timeSpan = new TimeSpan(0, 3, 4, 5);

        // Act
        var result = timeSpan.ToCronExpression();

        // Assert
        result.Should().Be("5 4 */3 * * *");
    }

    [Fact]
    public void ToCronExpression_WhenTimeSpanContainsMinutes_ShouldReturnMinuteCronExpression()
    {
        // Arrange
        var timeSpan = new TimeSpan(0, 0, 4, 5);

        // Act
        var result = timeSpan.ToCronExpression();

        // Assert
        result.Should().Be("5 */4 * * * *");
    }

    [Fact]
    public void ToCronExpression_WhenTimeSpanContainsOnlySeconds_ShouldReturnSecondCronExpression()
    {
        // Arrange
        var timeSpan = new TimeSpan(0, 0, 0, 5);

        // Act
        var result = timeSpan.ToCronExpression();

        // Assert
        result.Should().Be("0/5 * * * * *");
    }

    [Fact]
    public void ToCronExpression_WhenTimeSpanContainsOnlyMilliseconds_ShouldThrowArgumentException()
    {
        // Arrange
        var timeSpan = TimeSpan.FromMilliseconds(500);

        // Act
        var action = () => timeSpan.ToCronExpression();

        // Assert
        action.Should()
            .Throw<ArgumentException>()
            .WithMessage("TimeSpan must be greater than zero seconds*");
    }

    [Fact]
    public void ToCronExpression_WhenTimeSpanIsZero_ShouldThrowArgumentException()
    {
        // Arrange
        var timeSpan = TimeSpan.Zero;

        // Act
        var action = () => timeSpan.ToCronExpression();

        // Assert
        action.Should()
            .Throw<ArgumentException>()
            .WithMessage("TimeSpan must be greater than zero seconds*");
    }
}
