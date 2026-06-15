using EFlow.Booking.Application.Students.Commands.Import;
using EFlow.Booking.WebApi.Contracts.Students;
using EFlow.Booking.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EFlow.Booking.WebApi.Controllers.Internal;

[ApiController]
[Route("api/internal/import")]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class InternalStudentsController(ISender sender) : ControllerBase
{
    private const long MaxImportBodyBytes = 100 * 1024 * 1024;

    [HttpPost("students")]
    [Authorize(Policy = InternalAuthorizationPolicies.DataImport)]
    [Consumes("application/json")]
    [RequestSizeLimit(MaxImportBodyBytes)]
    [ProducesResponseType(typeof(StudentsImportResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ImportStudents(
        [FromQuery] Guid groupId,
        [FromBody] ImportStudentsRequest request,
        CancellationToken cancellationToken)
    {
        if (Request.ContentLength > MaxImportBodyBytes)
            return StatusCode(StatusCodes.Status413PayloadTooLarge);

        var command = new ImportStudentsCommand
        {
            GroupId = groupId,
            Students = request.Students
        };

        var result = await sender.Send(command, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            Ok(result.Value);
    }
}
