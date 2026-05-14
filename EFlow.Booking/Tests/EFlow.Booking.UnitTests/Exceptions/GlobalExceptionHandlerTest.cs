using System.Text.Json;
using EFlow.Booking.WebApi.Middleware;
using EFlow.Common.Domain;
using EFlow.Common.Domain.Exceptions;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace EFlow.Booking.UnitTests.Exceptions;

public class GlobalExceptionHandlerTest
{
    [Fact]
    public async Task TryHandleAsync_WhenGeneralException_ShouldReturn500()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        var handler = new GlobalExceptionHandler(loggerMock.Object);
        var context = new DefaultHttpContext();
        var exception = new Exception("Sensitive data in error message");
        var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        // Act
        var result = await handler.TryHandleAsync(context, exception, CancellationToken.None);
        responseStream.Seek(0, SeekOrigin.Begin);

        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(responseStream);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(500);
        problem.Should().NotBeNull();
        problem.Status.Should().Be(500);
        problem.Title.Should().Contain("Internal server error");
        problem.Title.Should().NotContain("Sensitive data in error message");
    }

    [Fact]
    public async Task TryHandleAsync_ShouldReturn422_ForValidationException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        var handler = new GlobalExceptionHandler(loggerMock.Object);
        var context = new DefaultHttpContext();
        var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        var exception = new ValidationException(
            new List<ValidationFailure>
            {
                new("Field1", "Field1 is required") { ErrorCode = "NotEmptyValidator" },
                new("Field2", "Field2 must be a number") { ErrorCode = "PredicateValidator" }
            });

        // Act
        var result = await handler.TryHandleAsync(context, exception, CancellationToken.None);
        responseStream.Seek(0, SeekOrigin.Begin);
        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(responseStream);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(422);
        problem.Should().NotBeNull();
        problem.Status.Should().Be(422);
        problem.Title.Should().Be("Validation Error");
        problem.Detail.Should().BeNull();

        problem.Extensions["code"].Should().BeOfType<JsonElement>()
            .Which.GetString().Should().Be("Validation.Field1.NotEmptyValidator");
        problem.Extensions.Should().NotContainKey("errors");
    }

    [Fact]
    public async Task TryHandleAsync_ShouldReturn422_ForBusinessRuleValidationException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        var handler = new GlobalExceptionHandler(loggerMock.Object);
        var context = new DefaultHttpContext();
        var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        var exception = new BusinessRuleValidationException(new TestBusinessRule());

        // Act
        var result = await handler.TryHandleAsync(context, exception, CancellationToken.None);
        responseStream.Seek(0, SeekOrigin.Begin);
        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(responseStream);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(422);
        problem.Should().NotBeNull();
        problem.Status.Should().Be(422);
        problem.Title.Should().Be("Business Rule Violation");
        problem.Detail.Should().BeNull();

        problem.Extensions["code"].Should().BeOfType<JsonElement>()
            .Which.GetString().Should().Be("BusinessRule.TestBusinessRule");
        problem.Extensions.Should().NotContainKey("fallbackCode");
    }

    private sealed class TestBusinessRule : IBusinessRule
    {
        public string Message => "Test business rule message";

        public bool IsBroken() => true;
    }
}
