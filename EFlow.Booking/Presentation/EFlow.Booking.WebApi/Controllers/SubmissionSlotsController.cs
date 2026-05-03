using System.Security.Claims;
using EFlow.Booking.Application.SubmissionSlots.Commands;
using EFlow.Booking.Application.SubmissionSlots.Commands.Update;
using EFlow.Booking.Application.SubmissionSlots.Queries;
using EFlow.Booking.Application.SubmissionSlots.Queries.GetByTeacherId;
using EFlow.Booking.Contracts.SubmissionSlots;
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
    [Authorize(Roles = $"{Identity.Roles.Admin},{Identity.Roles.Teacher}")]
    public async Task<IActionResult> CreateSlot([FromBody] CreateSubmissionSlotRequest request, CancellationToken cancellationToken)
    {
        if (!User.IsInRole(Identity.Roles.Admin) &&
            request.TeacherId.ToString() != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
            return Problem(
                title: "Forbidden",
                detail: "You can only create slots for yourself.",
                statusCode: StatusCodes.Status403Forbidden);

        var command = new CreateSubmissionSlotCommand
        {
            SubjectId = request.SubjectId,
            TeacherId = request.TeacherId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            MaxStudents = request.MaxStudents,
            AllowAllGroups = request.AllowAllGroups,
            AllowedGroupIds = request.AllowedGroupIds,
            Location = request.Location,
            Comment = request.Comment
        };

        var result = await sender.Send(command, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            CreatedAtAction(nameof(GetSlot), new { id = result.Value }, null);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(SubmissionSlotView), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSlot(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSubmissionSlotByIdQuery { Id = id }, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            Ok(result.Value);
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<SubmissionSlotView>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllSlots(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllSubmissionSlotsQuery(), cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            Ok(result.Value);
    }

    [HttpGet("reminder-snapshot")]
    [AllowAnonymous] // TODO: фиксануть авторизацию
    public async Task<IActionResult> GetReminderSnapshot(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSubmissionSlotReminderSnapshotQuery(), cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            Ok(result.Value);
    }

    [HttpGet("by-subject/{subjectId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<SubmissionSlotView>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSlotsBySubject(Guid subjectId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSubmissionSlotsBySubjectIdQuery { SubjectId = subjectId }, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            Ok(result.Value);
    }

    [HttpGet("by-teacher/{teacherId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<SubmissionSlotView>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSlotsByTeacher(Guid teacherId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSubmissionSlotsByTeacherIdQuery { TeacherId = teacherId }, cancellationToken);
        
        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            Ok(result.Value);
    }

    [HttpGet("available")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<SubmissionSlotView>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableSlots([FromQuery] GetAvailableSubmissionSlotsRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAvailableSubmissionSlotsQuery { FromDate = request.FromDate }, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            Ok(result.Value);
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = $"{Identity.Roles.Admin},{Identity.Roles.Teacher}")]
    public async Task<IActionResult> UpdateSlot(
        Guid id,
        [FromBody] UpdateSubmissionSlotRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.IsInRole(Identity.Roles.Admin))
        {
            var getSlotResult = await sender.Send(new GetSubmissionSlotByIdQuery { Id = id }, cancellationToken);
            
            if (getSlotResult.IsFailed)
                return getSlotResult.Errors[0].ToProblemDetails();
            
            if (getSlotResult.Value.Teacher!.Id.ToString() != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
                return Problem(
                    title: "Forbidden",
                    detail: "You can only update your own slots.",
                    statusCode: StatusCodes.Status403Forbidden);
        }
        
        var command = new UpdateSubmissionSlotCommand
        {
            Id = id,
            TeacherId = request.TeacherId,
            SubjectId = request.SubjectId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            MaxStudents = request.MaxStudents,
            AllowAllGroups = request.AllowAllGroups,
            AllowedGroupIds = request.AllowedGroupIds,
            Location = request.Location,
            Comment = request.Comment
        };

        var result = await sender.Send(command, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = $"{Identity.Roles.Admin},{Identity.Roles.Teacher}")]
    public async Task<IActionResult> DeleteSlot(Guid id, CancellationToken cancellationToken)
    {
        if (!User.IsInRole(Identity.Roles.Admin))
        {
            var getSlotResult = await sender.Send(new GetSubmissionSlotByIdQuery { Id = id }, cancellationToken);

            if (getSlotResult.IsFailed)
                return NoContent();
            
            if (getSlotResult.Value.Teacher!.Id.ToString() != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
                return Problem(
                    title: "Forbidden",
                    detail: "You can only delete your own slots.",
                    statusCode: StatusCodes.Status403Forbidden);
        }

        var result = await sender.Send(new DeleteSubmissionSlotCommand { Id = id }, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            NoContent();
    }

    [HttpPost("{id:guid}/admissions/{studentId:guid}")]
    [Authorize(Roles = $"{Identity.Roles.Admin},{Identity.Roles.Teacher}")]
    public async Task<IActionResult> AddAdmission(
        Guid id,
        Guid studentId,
        CancellationToken cancellationToken)
    {
        if (!User.IsInRole(Identity.Roles.Admin))
        {
            var getSlotResult = await sender.Send(new GetSubmissionSlotByIdQuery { Id = id }, cancellationToken);

            if (getSlotResult.IsFailed)
                return getSlotResult.Errors[0].ToProblemDetails();
            
            if (getSlotResult.Value.Teacher!.Id.ToString() != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
                return Problem(
                    title: "Forbidden",
                    detail: "You can only add admissions to your own slots.",
                    statusCode: StatusCodes.Status403Forbidden);
        }
        
        var result = await sender.Send(
            new AddAdmissionCommand
            {
                SlotId = id,
                StudentId = studentId,
            },
            cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            Ok(result.Value);
    }

    [HttpDelete("{id:guid}/admissions/{studentId:guid}")]
    [Authorize(Roles = $"{Identity.Roles.Admin},{Identity.Roles.Teacher}")]
    public async Task<IActionResult> RemoveAdmission(
        Guid id,
        Guid studentId,
        CancellationToken cancellationToken)
    {
        if (!User.IsInRole(Identity.Roles.Admin))
        {
            var getSlotResult = await sender.Send(new GetSubmissionSlotByIdQuery { Id = id }, cancellationToken);

            if (getSlotResult.IsFailed)
                return getSlotResult.Errors[0].ToProblemDetails();
            
            if (getSlotResult.Value.Teacher!.Id.ToString() != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
                return Problem(
                    title: "Forbidden",
                    detail: "You can only remove admissions to your own slots.",
                    statusCode: StatusCodes.Status403Forbidden);
        }

        var result = await sender.Send(
            new RemoveAdmissionCommand
            {
                SlotId = id,
                StudentId = studentId,
            },
            cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            NoContent();
    }

    [HttpPut("{id:guid}/notification-settings")]
    [Authorize]
    public async Task<IActionResult> UpdateNotificationSettings(
        Guid id,
        [FromBody] UpdateSubmissionSlotNotificationSettingsRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.IsInRole(Identity.Roles.Admin))
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (currentUserId != request.UserId.ToString())
                return Problem(
                    title: "Forbidden",
                    detail: "You can only update your own notification settings.",
                    statusCode: StatusCodes.Status403Forbidden);
        }

        var result = await sender.Send(
            new UpdateNotificationSettingsCommand
            {
                SlotId = id,
                UserId = request.UserId,
                SubmissionRemindTimes = request.SubmissionRemindTimes,
                BookingNotificationMode = request.BookingNotificationMode
            },
            cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            NoContent();
    }
}
