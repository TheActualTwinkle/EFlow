using System.Security.Claims;
using EFlow.Booking.Application.Students.Commands;
using EFlow.Booking.Application.Students.Commands.Update;
using EFlow.Booking.Application.Students.Queries;
using EFlow.Booking.Domain;
using EFlow.Booking.WebApi.Contracts.Students;
using EFlow.Booking.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EFlow.Booking.WebApi.Controllers;

[ApiController]
[Route("api/students")]
public class StudentsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = Identity.Roles.Admin)]
    public async Task<IActionResult> CreateStudent([FromBody] CreateStudentRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateStudentCommand
        {
            UserName = request.UserName,
            Password = request.Password,
            Email = request.Email,
            GroupId = request.GroupId,
            FirstName = request.FirstName,
            MiddleName = request.MiddleName,
            LastName = request.LastName,
            BirthDate = request.BirthDate
        };

        var result = await sender.Send(command, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            CreatedAtAction(nameof(GetStudent), new { id = result.Value }, null);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetStudent(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStudentByIdQuery { Id = id }, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            Ok(result.Value);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllStudents(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllStudentsQuery(), cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            Ok(result.Value);
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = $"{Identity.Roles.Admin},{Identity.Roles.Student}")]
    public async Task<IActionResult> UpdateStudent(
        Guid id,
        [FromBody] UpdateStudentRequest request,
        CancellationToken cancellationToken)
    {
        if (User.IsInRole(Identity.Roles.Student))
        {
            var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (studentId != id.ToString())
                return Problem(
                    title: "Forbidden",
                    detail: "You can only update your own profile.",
                    statusCode: StatusCodes.Status403Forbidden
                );
        }

        var command = new UpdateStudentCommand
        {
            Id = id,
            GroupId = request.GroupId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName,
            BirthDate = request.BirthDate
        };

        var result = await sender.Send(command, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Identity.Roles.Admin)]
    public async Task<IActionResult> DeleteStudent(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteStudentCommand { Id = id }, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            NoContent();
    }
}
