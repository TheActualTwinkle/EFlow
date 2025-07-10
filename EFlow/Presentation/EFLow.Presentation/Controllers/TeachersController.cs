using System.Security.Claims;
using EFlow.Application.Teachers.Commands;
using EFlow.Application.Teachers.Commands.Update;
using EFlow.Application.Teachers.Queries;
using EFlow.Domain.Models;
using EFLow.Presentation.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EFLow.Presentation.Controllers;

[ApiController]
[Route("api/teachers")]
public class TeachersController(ISender sender) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = Identity.Roles.Admin)]
    public async Task<IActionResult> CreateTeacher([FromBody] CreateTeacherCommand command, CancellationToken cancellationToken)
    {
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
        [FromBody] UpdateTeacherCommand command,
        CancellationToken cancellationToken)
    {
        if (User.IsInRole(Identity.Roles.Teacher))
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (teacherId != id.ToString())
                return Forbid("You can only update your own profile.");
        }

        var result = await sender.Send(command with { Id = id }, cancellationToken);

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