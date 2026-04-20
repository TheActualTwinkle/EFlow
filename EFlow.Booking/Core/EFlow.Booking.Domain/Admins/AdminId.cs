using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Admins;

public sealed record AdminId(Guid Value) : TypedIdValueBase(Value);