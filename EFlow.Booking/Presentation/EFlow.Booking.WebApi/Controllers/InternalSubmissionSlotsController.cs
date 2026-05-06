using EFlow.Booking.Application.SubmissionSlots.Queries;
using EFlow.Booking.Contracts.SubmissionSlots;
using EFlow.Booking.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EFlow.Booking.WebApi.Controllers;

[ApiController]
[Route("api/internal/submission-slots")]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class InternalSubmissionSlotsController(ISender sender) : ControllerBase
{
    [HttpGet("reminder-snapshot")]
    [Authorize(Policy = InternalAuthorizationPolicies.Notifications)]
    [ProducesResponseType(typeof(IEnumerable<SubmissionSlotReminderSnapshotView>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReminderSnapshot(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSubmissionSlotReminderSnapshotQuery(), cancellationToken);

        return result.IsFailed ?
            result.Errors[0].ToProblemDetails() :
            Ok(result.Value);
    }
}
