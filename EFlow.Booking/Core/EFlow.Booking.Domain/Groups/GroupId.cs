using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Groups;

public sealed record GroupId(Guid Value) : TypedIdValueBase(Value);