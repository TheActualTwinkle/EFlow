using EFlow.Booking.Domain;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Common.IntegrationEvents.Booking.Models;
using Microsoft.AspNetCore.Identity;

namespace EFlow.Booking.Application.Common.Mappers;

internal static class SubmissionSlotNotificationIntegrationMapper
{
    internal static async Task<NotificationRecipient[]> MapRecipientsAsync(
        SubmissionSlot slot,
        UserManager<Identity> userManager)
    {
        var recipients = slot.GetNotificationRecipients();
        var result = new List<NotificationRecipient>(recipients.Count);

        foreach (var recipient in recipients)
        {
            var user = await userManager.FindByIdAsync(recipient.UserId.ToString());

            result.Add(
                new NotificationRecipient
                {
                    UserId = recipient.UserId,
                    Email = user?.Email
                });
        }

        return result.ToArray();
    }
}
