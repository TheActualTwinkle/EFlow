using System.Security.Claims;
using EFlow.Booking.Application.Teachers.Commands;
using EFlow.Booking.Application.Teachers.Queries;
using EFlow.Booking.Domain;
using EFlow.Booking.WebApi.Contracts.Teachers;
using EFlow.Booking.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EFlow.Booking.WebApi.Controllers;

[ApiController]
[Route("api/teachers")]
public class TeachersController(ISender sender) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = Identity.Roles.Admin)]
    public async Task<IActionResult> CreateTeacher([FromBody] CreateTeacherRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateTeacherCommand
        {
            UserName = request.UserName,
            Password = request.Password,
            Email = request.Email,
            FirstName = request.FirstName,
            MiddleName = request.MiddleName,
            LastName = request.LastName,
            BirthDate = request.BirthDate
        };

        var result = await sender.Send(command, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            CreatedAtAction(nameof(GetTeacher), new { id = result.Value }, null);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetTeacher(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTeacherByIdQuery { Id = id }, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            Ok(result.Value);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllTeachers(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllTeachersQuery(), cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            Ok(result.Value);
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = $"{Identity.Roles.Admin},{Identity.Roles.Teacher}")]
    public async Task<IActionResult> UpdateTeacher(
        Guid id,
        [FromBody] UpdateTeacherRequest request,
        CancellationToken cancellationToken)
    {
        if (User.IsInRole(Identity.Roles.Teacher))
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (teacherId != id.ToString())
                return Problem(
                    title: "Forbidden",
                    detail: "You can only update your own profile.",
                    statusCode: StatusCodes.Status403Forbidden
                );
        }

        var command = new UpdateTeacherCommand
        {
            Id = id,
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
    public async Task<IActionResult> DeleteTeacher(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteTeacherCommand { Id = id }, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            NoContent();
    }
}
