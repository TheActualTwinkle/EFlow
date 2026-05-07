namespace EFlow.Booking.Caching.Settings;

public sealed record CacheSettings
{
    public const string SectionName = "CacheSettings";
    
    public required TimeSpan DefaultExpirationTime { get; init; }
}