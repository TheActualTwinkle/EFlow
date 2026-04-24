using EFlow.Booking.IntegrationEvents.BookingRecords;
using EFlow.Booking.IntegrationEvents.SubmissionSlots;
using EFlow.Booking.IntegrationEvents.SubmissionSlots.Notifications;
using EFlow.Common.Extensions;
using EFlow.Common.Messaging.Factories;
using EFlow.Common.Messaging.Settings;
using EFlow.Notifications.Services.NotificationServices.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EFlow.Notifications.Services.NotificationServices;

public sealed class BookingNotificationWorkflowHostedService(IServiceScopeFactory serviceScopeFactory) : IHostedService
{
    private readonly List<IDisposable> _subscriptions = [];
    private IServiceScope? _scope;
    private CancellationTokenSource? _stoppingCts;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _scope = serviceScopeFactory.CreateScope();
        _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var consumerFactory = _scope.ServiceProvider.GetRequiredService<ICommitLogConsumerFactory>();
        var notificationService = _scope.ServiceProvider.GetRequiredService<IEmailNotificationService>();

        _subscriptions.Add(
            consumerFactory
                .Create<Guid, BookingCreatedIntegrationEvent>(CreateConsumerSettings("booking-created-notification-workflow"))
                .FromTopic(KafkaTopics.BookingCreatedTopic)
                .DoAsync(message => HandleBookingCreatedAsync(message, notificationService, _stoppingCts.Token))
                .Subscribe());

        _subscriptions.Add(
            consumerFactory
                .Create<Guid, BookingCancelledIntegrationEvent>(CreateConsumerSettings("booking-cancelled-notification-workflow"))
                .FromTopic(KafkaTopics.BookingCancelledTopic)
                .DoAsync(message => HandleBookingCancelledAsync(message, notificationService, _stoppingCts.Token))
                .Subscribe());

        _subscriptions.Add(
            consumerFactory
                .Create<Guid, SubmissionSlotUpdatedIntegrationEvent>(CreateConsumerSettings("submission-slot-updated-notification-workflow"))
                .FromTopic(KafkaTopics.SubmissionSlotUpdatedTopic)
                .DoAsync(message => HandleSubmissionSlotUpdatedAsync(message, notificationService, _stoppingCts.Token))
                .Subscribe());

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _stoppingCts?.Cancel();

        foreach (var subscription in _subscriptions)
            subscription.Dispose();

        _subscriptions.Clear();
        _scope?.Dispose();

        return Task.CompletedTask;
    }

    private static KafkaConsumerSettings CreateConsumerSettings(string groupId) =>
        new()
        {
            GroupId = groupId,
            AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Latest
        };

    private static async Task HandleBookingCreatedAsync(
        BookingCreatedIntegrationEvent message,
        IEmailNotificationService notificationService,
        CancellationToken cancellationToken)
    {
        foreach (var recipient in message.Recipients)
        {
            if (ShouldNotifyAboutNewBooking(recipient))
                await notificationService.SendAsync(
                    new NotificationMessage
                    {
                        RecipientUserId = recipient.UserId,
                        RecipientEmail = recipient.Email,
                    Subject = "Defense booking created",
                    Body = $"Student {message.StudentId} booked defense slot {message.SlotId} at {message.SlotStartTime:O}."
                },
                cancellationToken);
        }
    }

    private static async Task HandleBookingCancelledAsync(
        BookingCancelledIntegrationEvent message,
        IEmailNotificationService notificationService,
        CancellationToken cancellationToken)
    {
        foreach (var recipient in message.Recipients.Where(ShouldNotifyAboutCancellation))
            await notificationService.SendAsync(
                new NotificationMessage
                {
                    RecipientUserId = recipient.UserId,
                    RecipientEmail = recipient.Email,
                    Subject = "Defense booking cancelled",
                    Body = $"Student {message.StudentId} cancelled booking {message.BookingRecordId} for slot {message.SlotId}."
                },
                cancellationToken);
    }

    private static async Task HandleSubmissionSlotUpdatedAsync(
        SubmissionSlotUpdatedIntegrationEvent message,
        IEmailNotificationService notificationService,
        CancellationToken cancellationToken)
    {
        foreach (var recipient in message.Recipients)
        {
            await notificationService.SendAsync(
                new NotificationMessage
                {
                    RecipientUserId = recipient.UserId,
                    RecipientEmail = recipient.Email,
                    Subject = "",
                    Body = $"Defense slot {message.SlotId} was updated. New start time: {message.SlotStartTime:O}."
                },
                cancellationToken);
        }
    }

    private static bool ShouldNotifyAboutNewBooking(SubmissionSlotNotificationRecipientIntegration recipient) =>
        (recipient.BookingNotificationMode ?? BookingNotificationModeIntegration.All) is
            BookingNotificationModeIntegration.All or
            BookingNotificationModeIntegration.OnlyNewBooking;

    private static bool ShouldNotifyAboutCancellation(SubmissionSlotNotificationRecipientIntegration recipient) =>
        (recipient.BookingNotificationMode ?? BookingNotificationModeIntegration.All) is
            BookingNotificationModeIntegration.All or
            BookingNotificationModeIntegration.OnlyCancellation;
}
