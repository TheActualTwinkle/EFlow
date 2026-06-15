using Microsoft.Extensions.Hosting;

namespace EFlow.Common.Extensions;

public static class ApiGeneratorExtensions
{
    extension(IHostEnvironment hostEnvironment)
    {
        public bool IsOpenApiGenerator() =>
            hostEnvironment.IsEnvironment("OpenApiGenerator");
    }
}