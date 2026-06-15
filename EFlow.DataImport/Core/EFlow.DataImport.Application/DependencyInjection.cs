using EFlow.DataImport.Application.Services.Students;
using Microsoft.Extensions.DependencyInjection;

namespace EFlow.DataImport.Application;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApplication()
        {
            services.AddScoped<IStudentImportWorker, StudentImportWorker>();

            return services;
        }
    }
}
