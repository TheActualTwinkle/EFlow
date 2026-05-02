using EFlow.Booking.Application.Admins.Commands;
using EFlow.Booking.Application.Admins.Queries;
using EFlow.Booking.Application.Services.AdminInitialing.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EFlow.Booking.Application.Services.AdminInitialing;

public sealed class AdminInitializer(
    ISender sender,
    IOptions<AdminConfiguration> adminConfiguration,
    ILogger<AdminInitializer> logger)
    : IAdminInitializer
{
    public async Task InitializeAsync()
    {
        logger.LogDebug("Trying to create admin user if not exists...");
        
        var configuration = adminConfiguration.Value;

        var admins = await sender.Send(new GetAllAdminsQuery());
        
        if (admins.IsFailed)
        {
            logger.LogCritical("Failed to retrieve admins: {Error}", admins.Errors);
            
            throw new InvalidOperationException($"Failed to retrieve admins. Errors: {admins.Errors}");
        }
        
        if (admins.Value.Any())
        {
            logger.LogInformation(
                "Skipping admin creation. Found existing admins: {Ids}",
                string.Join(", ", admins.Value.Select(a => a.Id)));
            
            return;
        }
        
        var result = await sender.Send(
            new CreateAdminCommand
            {
                UserName = configuration.Username,
                Password = configuration.Password,
                Email = configuration.Email
            });

        if (result.IsFailed)
        {
            logger.LogCritical("Failed to create admin: {Error}", result.Errors);
            
            throw new InvalidOperationException($"Failed to create admin. Errors: {result.Errors}");
        }
        
        logger.LogInformation("Default admin user '{UserName}' created successfully.", configuration.Username);
    }
}