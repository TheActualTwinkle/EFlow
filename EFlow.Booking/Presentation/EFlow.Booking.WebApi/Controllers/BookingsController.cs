using System.Security.Claims;
using EFlow.Booking.Application.BookingRecords.Commands;
using EFlow.Booking.Application.BookingRecords.Queries;
using EFlow.Booking.Application.BookingRecords.Queries.GetNotBookedStudents;
using EFlow.Booking.Contracts.BookingRecords;
using EFlow.Booking.Contracts.Students;
using EFlow.Booking.Domain;
using EFlow.Booking.WebApi.Contracts.Bookings;
using EFlow.Booking.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EFlow.Booking.WebApi.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = $"{Identity.Roles.Admin},{Identity.Roles.Student}")]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var command = new BookToSlotCommand
        {
            StudentId = request.StudentId,
            SlotId = request.SlotId
        };

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
    [ProducesResponseType(typeof(BookingRecordView), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBooking(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetBookingRecordByIdQuery { Id = id }, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            Ok(result.Value);
    }

    [HttpGet]
    [Authorize(Roles = Identity.Roles.Admin)]
    [ProducesResponseType(typeof(IEnumerable<BookingRecordView>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllBookings(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllBookingRecordsQuery(), cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            Ok(result.Value);
    }

    [HttpGet("by-student/{studentId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<BookingRecordView>), StatusCodes.Status200OK)]
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

        var result = await sender.Send(new GetBookingRecordsByStudentIdQuery { StudentId = studentId }, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            Ok(result.Value);
    }

    [HttpGet("by-slot/{slotId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<BookingRecordView>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBookingsBySlot(
        Guid slotId,
        [FromQuery] bool fetchGroups = false,
        CancellationToken cancellationToken = new())
    {
        var result = await sender.Send(
            new GetBookingRecordsBySlotIdQuery
            {
                SlotId = slotId,
                FetchStudentsGroups = fetchGroups
            },
            cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            Ok(result.Value);
    }
    
    [HttpGet("{slotId:guid}/not-booked-students")]
    [Authorize(Roles = $"{Identity.Roles.Admin},{Identity.Roles.Teacher}")]
    [ProducesResponseType(typeof(NotBookedStudentsView), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotBookedStudentsForSlot(Guid slotId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetNotBookedStudentsQuery { SlotId = slotId }, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            Ok(result.Value);
    }


    [HttpDelete("{id:guid}")]
    [Authorize(Roles = $"{Identity.Roles.Admin},{Identity.Roles.Student}")]
    public async Task<IActionResult> DeleteBooking(Guid id, CancellationToken cancellationToken)
    {
        if (User.IsInRole(Identity.Roles.Student))
        {
            var bookingResult = await sender.Send(new GetBookingRecordByIdQuery { Id = id }, cancellationToken);

            if (bookingResult.IsFailed)
                return bookingResult.Errors[0].ToProblemDetails();

            var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (studentId != bookingResult.Value.Student!.Id.ToString())
                return Problem(
                    title: "Forbidden",
                    detail: "You can only delete your own bookings",
                    statusCode: StatusCodes.Status403Forbidden
                );
        }

        var result = await sender.Send(new CancelBookingCommand { Id = id }, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            NoContent();
    }
}
