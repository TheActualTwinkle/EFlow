using EFlow.Booking.Application.Common.Errors;
using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace EFlow.Booking.WebApi.Extensions;

public static class ProblemDetailsExtensions
{
    public static IActionResult ToProblemDetails(this IError error)
    {
        ArgumentNullException.ThrowIfNull(error);

        var statusCode = GetStatusCode(error);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = error.Message,
            Detail = error.Reasons?.FirstOrDefault()?.Message,
            Type = error.GetType().Name
        };

        if (error.Metadata is null)
            return new ObjectResult(problemDetails) { StatusCode = statusCode };

        foreach (var kvp in error.Metadata)
            problemDetails.Extensions.Add(kvp.Key, kvp.Value);

        return new ObjectResult(problemDetails) { StatusCode = statusCode };
    }

    private static int GetStatusCode(IError error) =>
        error switch
        {
            BadRequestError => StatusCodes.Status400BadRequest,
            NotFoundError => StatusCodes.Status404NotFound,
            UnprocessableEntityError => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status500InternalServerError
        };
}
