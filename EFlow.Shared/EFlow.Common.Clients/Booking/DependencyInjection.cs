using EFlow.Common.Clients.Booking.Authentication;
using EFlow.Common.Clients.Booking.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EFlow.Common.Clients.Booking;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddBookingServiceAuthentication(
            IConfigurationSection jwtSection,
            string subject,
            string name)
        {
            services
                .AddOptions<BookingServiceJwtOptions>()
                .Bind(jwtSection)
                .Validate(options => options.ExpireMinutes > 0, "Booking service JWT expire minutes must be positive.")
                .ValidateOnStart();

            services
                .AddOptions<BookingServiceIdentityOptions>()
                .Configure(options =>
                {
                    options.Subject = subject;
                    options.Name = name;
                })
                .Validate(options => !string.IsNullOrWhiteSpace(options.Subject), "Booking service subject is required.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.Name), "Booking service name is required.")
                .ValidateOnStart();

            services.AddTransient<BookingInternalAuthenticationHandler>();

            return services;
        }

        public IHttpClientBuilder AddBookingServiceHttpClient<TClient, TImplementation>(
            Func<IServiceProvider, Uri> getBaseAddress)
            where TClient : class
            where TImplementation : class, TClient =>
            services.AddHttpClient<TClient, TImplementation>((serviceProvider, httpClient) =>
            {
                httpClient.BaseAddress = getBaseAddress(serviceProvider);
            });
    }
}
