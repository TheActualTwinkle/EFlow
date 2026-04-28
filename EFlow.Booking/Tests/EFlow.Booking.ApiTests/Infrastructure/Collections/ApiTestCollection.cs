using EFlow.Booking.ApiTests.Infrastructure.Fixtures;

namespace EFlow.Booking.ApiTests.Infrastructure.Collections;

/// <summary>
/// Declares the shared xUnit collection used by Booking API tests.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class ApiTestCollection : ICollectionFixture<ApiTestStackFixture>
{
    /// <summary>
    /// Gets the collection name that binds tests to the shared Booking API fixture.
    /// </summary>
    public const string Name = "booking-api";
}