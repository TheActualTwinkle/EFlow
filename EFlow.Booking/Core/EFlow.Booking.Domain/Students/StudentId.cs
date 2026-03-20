using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Students;

public sealed record StudentId(Guid Value) : TypedIdValueBase(Value);