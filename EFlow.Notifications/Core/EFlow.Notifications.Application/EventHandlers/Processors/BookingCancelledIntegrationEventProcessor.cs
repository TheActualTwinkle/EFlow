using EFlow.Common.IntegrationEvents.Booking.BookingRecords;
using EFlow.Notifications.Application.Email.Interfaces;
using EFlow.Notifications.Application.Email.Models;
using EFlow.Notifications.Application.EventHandlers.Processors.Interfaces;
using EFlow.Notifications.Templates.Notifications.Interfaces;
using Microsoft.Extensions.Logging;

namespace EFlow.Notifications.Application.EventHandlers.Processors;

public sealed class BookingCancelledIntegrationEventProcessor(
    IBookingNotificationTemplateService templateService,
    IEmailNotificationService notificationService,
    Logger<BookingCancelledIntegrationEventProcessor> logger)
    : IIntegrationEventProcessor<BookingCancelledIntegrationEvent>
{
    public async Task ProcessAsync(BookingCancelledIntegrationEvent @event, CancellationToken cancellationToken = new())
    {
        var (subject, body) = await templateService.CreateBookingCancelledAsync(@event.BookingRecord, cancellationToken);

        foreach (var recipient in @event.NotificationRecipients)
        {
            if (recipient.Email is null)
            {
                logger.LogWarning("Recipient {UserId} does not have an email address. Skipping notification.", recipient.UserId);

                continue;
            }

            await notificationService.SendAsync(
                new NotificationMessage
                {
                    Subject = subject,
                    Body = body,
                    RecipientEmail = recipient.Email
                },
                cancellationToken);
        }
    }
}