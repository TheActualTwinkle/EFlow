using EFlow.Application.SubmissionSlots.Commands;
using EFlow.Application.SubmissionSlots.Commands.Update;
using EFlow.Application.SubmissionSlots.Queries;
using EFlow.Domain.Models;
using EFlow.Presentation.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EFlow.Presentation.Controllers;

[ApiController]
[Route("api/submission-slots")]
public class SubmissionSlotsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = Identity.Roles.Admin)]
    public async Task<IActionResult> CreateSlot([FromBody] CreateSubmissionSlotCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            CreatedAtAction(nameof(GetSlot), new { id = result.Value }, null);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetSlot(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSubmissionSlotByIdQuery { Id = id }, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            Ok(result.Value);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllSlots(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllSubmissionSlotsQuery(), cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            Ok(result.Value);
    }

    [HttpGet("by-subject/{subjectId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetSlotsBySubject(Guid subjectId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSubmissionSlotsBySubjectIdQuery { SubjectId = subjectId }, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            Ok(result.Value);
    }

    [HttpGet("available")]
    [Authorize]
    public async Task<IActionResult> GetAvailableSlots([FromQuery] DateTime fromDate, CancellationToken cancellationToken)
    {
        if (fromDate.Kind != DateTimeKind.Utc)
            fromDate = DateTime.SpecifyKind(fromDate, DateTimeKind.Utc);

        var result = await sender.Send(new GetAvailableSubmissionSlotsQuery { FromDate = fromDate }, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            Ok(result.Value);
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = Identity.Roles.Admin)]
    public async Task<IActionResult> UpdateSlot(
        Guid id,
        [FromBody] UpdateSubmissionSlotCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command with { Id = id }, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Identity.Roles.Admin)]
    public async Task<IActionResult> DeleteSlot(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteSubmissionSlotCommand { Id = id }, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            NoContent();
    }
}