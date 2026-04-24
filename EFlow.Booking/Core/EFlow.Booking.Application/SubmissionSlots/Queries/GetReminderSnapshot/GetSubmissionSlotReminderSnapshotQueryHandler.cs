using EFlow.Booking.Domain;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Booking.IntegrationEvents.SubmissionSlots.Notifications;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EFlow.Booking.Application.SubmissionSlots.Queries;

public sealed class GetSubmissionSlotReminderSnapshotQueryHandler(
    IUnitOfWork unitOfWork,
    UserManager<Identity> userManager)
    : IRequestHandler<GetSubmissionSlotReminderSnapshotQuery, Result<IEnumerable<SubmissionSlotReminderSnapshotDto>>>
{
    public async Task<Result<IEnumerable<SubmissionSlotReminderSnapshotDto>>> Handle(
        GetSubmissionSlotReminderSnapshotQuery request,
        CancellationToken cancellationToken)
    {
        var slots = await unitOfWork
            .GetRepository<ISubmissionSlotRepository>()
            .GetAllAsync(cancellationToken);

        var result = new List<SubmissionSlotReminderSnapshotDto>();

        foreach (var slot in slots)
        {
            var recipients = slot.GetNotificationRecipients();

            if (recipients.Count == 0)
                continue;

            var mappedRecipients = new List<SubmissionSlotReminderRecipientDto>(recipients.Count);

            foreach (var recipient in recipients)
            {
                var user = await userManager.FindByIdAsync(recipient.UserId.ToString());

                mappedRecipients.Add(
                    new SubmissionSlotReminderRecipientDto
                    {
                        UserId = recipient.UserId,
                        Email = user?.Email,
                        ReminderSchedules = recipient.ReminderSchedules
                            .Select(schedule => (ReminderScheduleIntegration)(int)schedule)
                            .ToArray()
                    });
            }

            result.Add(
                new SubmissionSlotReminderSnapshotDto
                {
                    SlotId = slot.Id.Value,
                    SlotStartTime = slot.GetStartTime(),
                    Recipients = mappedRecipients.ToArray()
                });
        }

        return Result.Ok<IEnumerable<SubmissionSlotReminderSnapshotDto>>(result);
    }
}
