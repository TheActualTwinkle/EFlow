using EFlow.Common.Clients.Booking;
using EFlow.Common.Clients.Booking.Authentication;
using EFlow.Common.Messaging;
using EFlow.Notifications.Messaging.Booking;
using EFlow.Notifications.Messaging.Booking.Interfaces;
using EFlow.Notifications.Messaging.Booking.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EFlow.Notifications.Messaging;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddMessaging(IConfiguration configuration) =>
            services.AddKafka(configuration, useDeadLetterQueue: true);

        public IServiceCollection AddBookingClient(IConfiguration configuration)
        {
            services.Configure<BookingReminderSettings>(configuration.GetSection(BookingReminderSettings.SectionName));

            services.AddBookingServiceAuthentication(
                configuration.GetRequiredSection("Jwt"),
                subject: "eflow-notifications",
                "EFlow.Notifications");

            services
                .AddBookingServiceHttpClient<IBookingClient, BookingClient>(serviceProvider =>
                    serviceProvider.GetRequiredService<IOptions<BookingReminderSettings>>().Value.BookingApiBaseUrl)
                .AddHttpMessageHandler<BookingInternalAuthenticationHandler>();

            return services;
        }
    }
}
