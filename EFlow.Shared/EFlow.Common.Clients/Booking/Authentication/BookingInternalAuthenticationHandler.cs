using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using EFlow.Common.Clients.Booking.Options;
using EFlow.Common.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EFlow.Common.Clients.Booking.Authentication;

public sealed class BookingInternalAuthenticationHandler(
    ISystemClock systemClock,
    IOptions<BookingServiceJwtOptions> jwtOptions,
    IOptions<BookingServiceIdentityOptions> identityOptions)
    : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken());

        return base.SendAsync(request, cancellationToken);
    }

    private string CreateToken()
    {
        var jwt = jwtOptions.Value;
        var identity = identityOptions.Value;
        var nowUtc = systemClock.UtcNow;

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            jwt.Issuer,
            jwt.Audience,
            [
                new Claim(JwtRegisteredClaimNames.Sub, identity.Subject),
                new Claim(ClaimTypes.NameIdentifier, identity.Subject),
                new Claim(ClaimTypes.Name, identity.Name),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ],
            nowUtc,
            nowUtc.AddMinutes(jwt.ExpireMinutes),
            credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}