using EFlow.Common.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EFlow.Booking.WebApi.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(
            exception,
            "Exception occurred: {Message}", exception.Message);

        var problemDetails = ProblemDetailsFactory.Get(exception);

        httpContext.Response.StatusCode = problemDetails.Status!.Value;

        await httpContext.Response
            .WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}

internal static class ProblemDetailsFactory
{
    public static ProblemDetails Get(Exception exception) =>
        exception switch
        {
            ValidationException validationException => CreateValidationProblemDetails(validationException),
            BusinessRuleValidationException businessRuleValidationException => CreateBusinessRuleProblemDetails(businessRuleValidationException),
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal server error."
            }
        };

    private static ProblemDetails CreateValidationProblemDetails(ValidationException exception) =>
        new()
        {
            Status = StatusCodes.Status422UnprocessableEntity,
            Title = "Validation Error",
            Extensions =
            {
                ["code"] = exception.Errors
                    .Select(error => $"Validation.{error.PropertyName}.{error.ErrorCode}")
                    .FirstOrDefault("Validation.Error")
            }
        };

    private static ProblemDetails CreateBusinessRuleProblemDetails(BusinessRuleValidationException exception) =>
        new()
        {
            Status = StatusCodes.Status422UnprocessableEntity,
            Title = "Business Rule Violation",
            Extensions =
            {
                ["code"] = $"BusinessRule.{exception.BrokenRule.GetType().Name}"
            }
        };
}