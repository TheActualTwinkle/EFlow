using System.Text;
using EFlow.Booking.Domain;
using EFlow.Booking.Persistence.DatabaseContext;
using EFlow.Booking.WebApi.Mapping;
using Mapster;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace EFlow.Booking.WebApi;

public static class DependencyInjection
{
    public static async Task<IApplicationBuilder> CreateRolesAsync(this IApplicationBuilder builder)
    {
        using var scope = builder.ApplicationServices.CreateScope();

        var services = scope.ServiceProvider;
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        foreach (var role in Identity.Roles.GetAll())
        {
            if (await roleManager.RoleExistsAsync(role))
                continue;

            await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }

        return builder;
    }

    public static IServiceCollection ConfigureIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity<Identity, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6; // TODO сделать нормальные ограничения
                options.Lockout.MaxFailedAccessAttempts = 3;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddAuthentication(options =>
            {
                options.DefaultScheme = "JWT_OR_COOKIE";
                options.DefaultChallengeScheme = "JWT_OR_COOKIE";
            })
            .AddCookie(
                CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);

                    options.Events.OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                        return Task.CompletedTask;
                    };

                    options.Events.OnRedirectToAccessDenied = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;

                        return Task.CompletedTask;
                    };
                })
            .AddJwtBearer(
                JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidAudience = configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!))
                    };
                })
            .AddPolicyScheme(
                "JWT_OR_COOKIE", "JWT_OR_COOKIE", options =>
                {
                    options.ForwardDefaultSelector = context =>
                    {
                        var authorization = context.Request.Headers.Authorization.FirstOrDefault();

                        return authorization?.StartsWith("Bearer ") == true ?
                            JwtBearerDefaults.AuthenticationScheme :
                            CookieAuthenticationDefaults.AuthenticationScheme;
                    };
                });

        services.AddAuthorization();

        return services;
    }
    
    public static IServiceCollection AddMapping(this IServiceCollection services)
    {
        TypeAdapterConfig.GlobalSettings.Apply(new MapsterRegister());
        TypeAdapterConfig.GlobalSettings.RequireExplicitMapping = true;
        TypeAdapterConfig.GlobalSettings.Default.IgnoreNonMapped(true);

        services.AddSingleton(TypeAdapterConfig.GlobalSettings);

        return services;
    }
}
