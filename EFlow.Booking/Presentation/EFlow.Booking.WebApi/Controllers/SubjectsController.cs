using EFlow.Booking.Application.Subjects.Commands;
using EFlow.Booking.Application.Subjects.Commands.Update;
using EFlow.Booking.Application.Subjects.Queries;
using EFlow.Booking.Contracts.Subjects;
using EFlow.Booking.Domain;
using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Domain.Subjects;
using EFlow.Booking.Domain.Teachers;
using EFlow.Booking.WebApi.Contracts.Subjects;
using EFlow.Booking.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EFlow.Booking.WebApi.Controllers;

[ApiController]
[Route("api/subjects")]
public class SubjectsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = Identity.Roles.Admin)]
    public async Task<IActionResult> CreateSubject([FromBody] CreateSubjectRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateSubjectCommand
        {
            Name = request.Name,
            TeacherId = request.TeacherId,
            GroupIds = request.GroupIds
        };

        var result = await sender.Send(command, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            CreatedAtAction(nameof(GetSubject), new { id = result.Value }, null);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(SubjectView), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubject(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSubjectByIdQuery { Id = id }, cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            Ok(result.Value);
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<SubjectView>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllSubjects(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllSubjectsQuery(), cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            Ok(result.Value);
    }

    [HttpGet("by-teacher/{teacherId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<SubjectView>), StatusCodes.Status200OK)]
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
        [FromBody] UpdateSubjectRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSubjectCommand
        {
            Id = id,
            Patch = new SubjectUpdatePatch
            {
                Name = request.Name,
                TeacherId = request.TeacherId.Map(teacherId => new TeacherId(teacherId)),
                GroupIds = request.GroupIds.Map(ICollection<GroupId> (groupIds) =>
                    groupIds.Select(groupId => new GroupId(groupId)).ToArray())
            }
        };

        var result = await sender.Send(command, cancellationToken);

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
