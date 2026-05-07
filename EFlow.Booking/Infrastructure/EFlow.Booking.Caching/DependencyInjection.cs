using EFlow.Booking.Caching.Interfaces;
using EFlow.Booking.Caching.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EFlow.Booking.Caching;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddCaching(IConfiguration configuration)
        {
            services.Configure<CacheSettings>(configuration.GetSection(CacheSettings.SectionName));
            
            services.AddSingleton<ICacheService, InMemoryCacheService>();
            
            return services;
        }
    }
}