using EFlow.Common.IntegrationTests.Infrastructure.Fixtures;

namespace EFlow.Common.IntegrationTests.Infrastructure.Collections;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class KafkaTestCollection : ICollectionFixture<KafkaTestStackFixture>
{
    public const string Name = "kafka";
}
