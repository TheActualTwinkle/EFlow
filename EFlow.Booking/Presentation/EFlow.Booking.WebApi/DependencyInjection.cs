using System.Text;
using System.Security.Claims;
using EFlow.Booking.Domain;
using EFlow.Booking.Persistence.DatabaseContext;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace EFlow.Booking.WebApi;

public static class DependencyInjection
{
    extension(IApplicationBuilder builder)
    {
        public async Task<IApplicationBuilder> CreateRolesAsync()
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
    }

    extension(IServiceCollection services)
    {
        public IServiceCollection ConfigureIdentity(IConfiguration configuration)
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
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
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
            });

            const string jwtOrCookieScheme = "JWT_OR_COOKIE";

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = jwtOrCookieScheme;
                    options.DefaultAuthenticateScheme = jwtOrCookieScheme;
                    options.DefaultChallengeScheme = jwtOrCookieScheme;
                    options.DefaultForbidScheme = jwtOrCookieScheme;
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
                    jwtOrCookieScheme, jwtOrCookieScheme, options =>
                    {
                        options.ForwardDefaultSelector = context =>
                        {
                            var authorization = context.Request.Headers.Authorization.FirstOrDefault();

                            return authorization?.StartsWith("Bearer ") == true ?
                                JwtBearerDefaults.AuthenticationScheme :
                                IdentityConstants.ApplicationScheme;
                        };
                    });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    InternalAuthorizationPolicies.Notifications,
                    policy => policy
                        .RequireAuthenticatedUser()
                        .RequireAssertion(context =>
                            context.User.HasClaim(ClaimTypes.NameIdentifier, "eflow-notifications") ||
                            context.User.HasClaim("sub", "eflow-notifications")));
                
                options.AddPolicy(
                    InternalAuthorizationPolicies.DataImport,
                    policy => policy
                        .RequireAuthenticatedUser()
                        .RequireAssertion(context =>
                            context.User.HasClaim(ClaimTypes.NameIdentifier, "eflow-data-import") ||
                            context.User.HasClaim("sub", "eflow-data-import")));
            });

            return services;
        }
    }
}
