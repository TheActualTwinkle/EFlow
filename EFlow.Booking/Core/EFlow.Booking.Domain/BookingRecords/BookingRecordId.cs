using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.BookingRecords;

public sealed record BookingRecordId(Guid Value) : TypedIdValueBase(Value);