using Microsoft.Extensions.DependencyInjection;

namespace EFlow.Booking.Caching;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddCaching()
        {
            // TODO: implement
            return services;
        }
    }
}