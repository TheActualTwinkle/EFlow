using EFlow.Booking.Application.Common.Markers;
using EFlow.Booking.Domain.Notifications;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Commands;

public record UpdateNotificationSettingsCommand : IRequest<Result>, ITransactionalRequest
{
    public required Guid SlotId { get; init; }

    public required Guid UserId { get; init; }

    public required ICollection<SubmissionRemindTime> SubmissionRemindTimes { get; init; }

    public BookingNotificationMode? BookingNotificationMode { get; init; }
}
