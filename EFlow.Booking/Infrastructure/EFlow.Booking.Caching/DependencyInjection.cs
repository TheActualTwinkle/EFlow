using EFlow.Booking.Caching.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EFlow.Booking.Caching;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddCaching(IConfiguration configuration)
        {
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, InMemoryCacheService>();
            
            return services;
        }
    }
}
