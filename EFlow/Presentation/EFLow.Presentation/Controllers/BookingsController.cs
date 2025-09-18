using System.Security.Claims;
using EFlow.Application.Bookings.Commands;
using EFlow.Application.Bookings.Commands.Update;
using EFlow.Application.Bookings.Queries;
using EFlow.Domain.Models;
using EFlow.Presentation.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EFlow.Presentation.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = $"{Identity.Roles.Admin},{Identity.Roles.Student}")]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingCommand command, CancellationToken cancellationToken)
    {
        if (User.IsInRole(Identity.Roles.Student))
        {
            var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (studentId != command.StudentId.ToString())
                return Problem(
                    title: "Forbidden",
                    detail: "You can only create bookings for yourself",
                    statusCode: StatusCodes.Status403Forbidden
                );
        }

        var result = await sender.Send(command, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            CreatedAtAction(nameof(GetBooking), new { id = result.Value }, null);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetBooking(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetBookingByIdQuery { Id = id }, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            Ok(result.Value);
    }

    [HttpGet]
    [Authorize(Roles = Identity.Roles.Admin)]
    public async Task<IActionResult> GetAllBookings(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllBookingsQuery(), cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            Ok(result.Value);
    }

    [HttpGet("by-student/{studentId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetBookingsByStudent(Guid studentId, CancellationToken cancellationToken)
    {
        if (User.IsInRole(Identity.Roles.Student))
        {
            var currentStudentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (currentStudentId != studentId.ToString())
                return Problem(
                    title: "Forbidden",
                    detail: "You can only view your own bookings",
                    statusCode: StatusCodes.Status403Forbidden
                );
        }

        var result = await sender.Send(new GetBookingsByStudentIdQuery { StudentId = studentId }, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            Ok(result.Value);
    }

    [HttpGet("by-slot/{slotId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetBookingsBySlot(Guid slotId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetBookingsBySlotIdQuery { SlotId = slotId }, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            Ok(result.Value);
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = Identity.Roles.Admin)]
    public async Task<IActionResult> UpdateBooking(
        Guid id,
        [FromBody] UpdateBookingCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command with { Id = id }, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = $"{Identity.Roles.Admin},{Identity.Roles.Student}")]
    public async Task<IActionResult> DeleteBooking(Guid id, CancellationToken cancellationToken)
    {
        if (User.IsInRole(Identity.Roles.Student))
        {
            var bookingResult = await sender.Send(new GetBookingByIdQuery { Id = id }, cancellationToken);

            if (bookingResult.IsFailed)
                return bookingResult.Errors[0].ToProblemDetails();

            var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (studentId != bookingResult.Value.StudentId.ToString())
                return Problem(
                    title: "Forbidden",
                    detail: "You can only delete your own bookings",
                    statusCode: StatusCodes.Status403Forbidden
                );
        }

        var result = await sender.Send(new DeleteBookingCommand { Id = id }, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            NoContent();
    }
}