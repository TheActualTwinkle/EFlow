using EFlow.DataImport.ApiTests.Infrastructure.Fixtures;

namespace EFlow.DataImport.ApiTests.Infrastructure.Collections;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class ApiTestCollection : ICollectionFixture<DataImportApiTestStackFixture>
{
    public const string Name = "data-import-api";
}
