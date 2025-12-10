using EFlow.Application.Subjects.Commands;
using EFlow.Application.Subjects.Commands.Update;
using EFlow.Application.Subjects.Queries;
using EFlow.Domain.Models;
using EFlow.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EFlow.WebApi.Controllers;

[ApiController]
[Route("api/subjects")]
public class SubjectsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = Identity.Roles.Admin)]
    public async Task<IActionResult> CreateSubject([FromBody] CreateSubjectCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            CreatedAtAction(nameof(GetSubject), new { id = result.Value }, null);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetSubject(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSubjectByIdQuery { Id = id }, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            Ok(result.Value);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllSubjects(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllSubjectsQuery(), cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            Ok(result.Value);
    }

    [HttpGet("by-teacher/{teacherId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetSubjectsByTeacher(Guid teacherId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSubjectsByTeacherIdQuery { TeacherId = teacherId }, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            Ok(result.Value);
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = Identity.Roles.Admin)]
    public async Task<IActionResult> UpdateSubject(
        Guid id,
        [FromBody] UpdateSubjectCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command with { Id = id }, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Identity.Roles.Admin)]
    public async Task<IActionResult> DeleteSubject(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteSubjectCommand { Id = id }, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            NoContent();
    }
}