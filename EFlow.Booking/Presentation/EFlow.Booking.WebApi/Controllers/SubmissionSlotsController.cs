using EFlow.Booking.Application.SubmissionSlots.Commands;
using EFlow.Booking.Application.SubmissionSlots.Commands.Update;
using EFlow.Booking.Application.SubmissionSlots.Queries;
using EFlow.Booking.Domain;
using EFlow.Booking.WebApi.Contracts.SubmissionSlots;
using EFlow.Booking.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EFlow.Booking.WebApi.Controllers;

[ApiController]
[Route("api/submission-slots")]
public class SubmissionSlotsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = Identity.Roles.Admin)]
    public async Task<IActionResult> CreateSlot([FromBody] CreateSubmissionSlotRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateSubmissionSlotCommand
        {
            SubjectId = request.SubjectId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            MaxStudents = request.MaxStudents,
            AllowAllGroups = request.AllowAllGroups,
            AllowedGroupIds = request.AllowedGroupIds,
            Location = request.Location
        };

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
    public async Task<IActionResult> GetAvailableSlots([FromQuery] GetAvailableSubmissionSlotsRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAvailableSubmissionSlotsQuery { FromDate = request.FromDate }, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            Ok(result.Value);
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = Identity.Roles.Admin)]
    public async Task<IActionResult> UpdateSlot(
        Guid id,
        [FromBody] UpdateSubmissionSlotRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSubmissionSlotCommand
        {
            Id = id,
            SubjectId = request.SubjectId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            MaxStudents = request.MaxStudents,
            Location = request.Location
        };

        var result = await sender.Send(command, cancellationToken);

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