using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EFlow.Notifications.Messaging.Booking.Settings;
using Microsoft.Extensions.Options;

namespace EFlow.Notifications.Messaging.Booking;

public sealed class BookingAuthenticationHandler(IOptions<BookingClientJwtSettings> options) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken());

        return base.SendAsync(request, cancellationToken);
    }

    private string CreateToken()
    {
        var settings = options.Value;
        var now = DateTimeOffset.UtcNow;
        var expires = now.AddMinutes(settings.ExpireMinutes);
        
        var header = Base64UrlEncode(
            JsonSerializer.SerializeToUtf8Bytes(
                new Dictionary<string, string>
                {
                    ["alg"] = "HS256",
                    ["typ"] = "JWT"
                }));
        
        var payload = Base64UrlEncode(
            JsonSerializer.SerializeToUtf8Bytes(
                new Dictionary<string, object>
                {
                    ["iss"] = settings.Issuer,
                    ["aud"] = settings.Audience,
                    ["sub"] = "eflow-notifications",
                    ["name"] = "EFlow.Notifications",
                    ["iat"] = now.ToUnixTimeSeconds(),
                    ["nbf"] = now.ToUnixTimeSeconds(),
                    ["exp"] = expires.ToUnixTimeSeconds()
                }));

        var unsignedToken = $"{header}.{payload}";
        var signature = Sign(unsignedToken, settings.Key);

        return $"{unsignedToken}.{signature}";
    }

    private static string Sign(string unsignedToken, string key)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(unsignedToken));

        return Base64UrlEncode(signature);
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
