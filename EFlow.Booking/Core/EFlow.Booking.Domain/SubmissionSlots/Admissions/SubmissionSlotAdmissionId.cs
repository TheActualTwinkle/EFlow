using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.SubmissionSlots.Admissions;

public sealed record SubmissionSlotAdmissionId(Guid Value) : TypedIdValueBase(Value);
