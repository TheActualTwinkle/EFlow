using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using EFlow.DataImport.Application.Models.Students;
using EFlow.DataImport.Application.Services.Students;
using EFlow.DataImport.Messaging.Booking.Abstractions;
using EFlow.DataImport.WebApi.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace EFlow.DataImport.WebApi.Controllers;

[ApiController]
[Route("api/csv")]
public sealed class CsvImportController(
    IStudentImportWorker studentImportWorker,
    IBookingCurrentUserClient bookingCurrentUserClient)
    : ControllerBase
{
    private const long MaxImportBodyBytes = 100 * 1024 * 1024;
    private const string AdminRole = "Admin";

    [HttpPost("students")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxImportBodyBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxImportBodyBytes)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ImportStudents(
        [FromQuery] [Required] Guid groupId,
        [FromForm] ImportStudentsRequest request,
        CancellationToken cancellationToken)
    {
        if (ValidateStudentImportRequest(groupId, request, out var actionResult))
            return actionResult;

        var authorizationHeader = Request.Headers.Authorization.ToString();
        var cookieHeader = Request.Headers.Cookie.ToString();

        var currentUser = await bookingCurrentUserClient.GetCurrentUserAsync(
            authorizationHeader,
            cookieHeader,
            cancellationToken);

        if ((int)currentUser.StatusCode is not StatusCodes.Status200OK)
            return StatusCode((int)currentUser.StatusCode, currentUser.Body);

        if (currentUser.User?.Roles.Contains(AdminRole) != true)
            return Forbid();

        await using var stream = request.File.OpenReadStream();

        var result = await studentImportWorker.ImportStudentsAsync(
            new StudentImportProxyRequest
            {
                GroupId = groupId,
                FileStream = stream,
                FileName = request.File.FileName,
                ContentType = request.File.ContentType,
                Fields = request.Fields.ToList(),
                HasHeaderRow = request.HasHeaderRow
            },
            cancellationToken);

        if (string.IsNullOrWhiteSpace(result.ContentType))
            return StatusCode((int)result.StatusCode, result.Body);

        return new ContentResult
        {
            StatusCode = (int)result.StatusCode,
            Content = result.Body ?? string.Empty,
            ContentType = result.ContentType
        };
    }

    private bool ValidateStudentImportRequest(
        Guid groupId,
        ImportStudentsRequest request,
        [NotNullWhen(true)] out IActionResult? actionResult)
    {
        actionResult = null;

        if (groupId == Guid.Empty)
        {
            actionResult = UnprocessableEntity("Group ID is required");

            return true;
        }

        if (request.File.Length == 0)
        {
            actionResult = UnprocessableEntity("CSV file is required");

            return true;
        }

        if (request.Fields.Length == 0)
        {
            actionResult = UnprocessableEntity("Column fields are required");

            return true;
        }

        return false;
    }
}