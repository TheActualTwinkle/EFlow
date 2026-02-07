using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Teachers;

public sealed record TeacherId(Guid Value) : TypedIdValueBase(Value);