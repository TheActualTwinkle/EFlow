using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.SubmissionSlots;

public sealed record SubmissionSlotId(Guid Value) : TypedIdValueBase(Value);