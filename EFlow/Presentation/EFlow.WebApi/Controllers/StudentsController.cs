using System.Security.Claims;
using EFlow.Application.Students.Commands;
using EFlow.Application.Students.Commands.Update;
using EFlow.Application.Students.Queries;
using EFlow.Domain.Models;
using EFlow.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EFlow.WebApi.Controllers;

[ApiController]
[Route("api/students")]
public class StudentsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = Identity.Roles.Admin)]
    public async Task<IActionResult> CreateStudent([FromBody] CreateStudentCommand command, CancellationToken cancellationToken)
    {
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
        [FromBody] UpdateStudentCommand command,
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

        var result = await sender.Send(command with { Id = id }, cancellationToken);

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