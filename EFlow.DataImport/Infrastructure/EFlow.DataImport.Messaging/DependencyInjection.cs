using EFlow.Common.Clients.Booking;
using EFlow.Common.Clients.Booking.Authentication;
using EFlow.Common.Clients.Booking.Options;
using EFlow.DataImport.Messaging.Booking.Abstractions;
using EFlow.DataImport.Messaging.Booking.Clients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EFlow.DataImport.Messaging;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddInfrastructureClients(IConfiguration configuration)
        {
            services.Configure<BookingApiOptions>(configuration.GetRequiredSection(BookingApiOptions.SectionName));

            services.AddBookingServiceAuthentication(
                configuration.GetRequiredSection("Jwt"),
                subject: "eflow-data-import",
                name: "EFlow.DataImport");

            services.AddBookingServiceHttpClient<IBookingCurrentUserClient, BookingCurrentUserClient>(
                serviceProvider => serviceProvider.GetRequiredService<IOptions<BookingApiOptions>>().Value.BaseUrl);

            services.AddBookingServiceHttpClient<IBookingStudentImportClient, BookingStudentImportClient>(
                serviceProvider => serviceProvider.GetRequiredService<IOptions<BookingApiOptions>>().Value.BaseUrl)
                .AddHttpMessageHandler<BookingInternalAuthenticationHandler>();

            return services;
        }
    }
}
