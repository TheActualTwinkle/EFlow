using System.Net;
using EFlow.Booking.ApiTests.Infrastructure.Collections;
using EFlow.Booking.ApiTests.Infrastructure.Contracts;
using EFlow.Booking.ApiTests.Infrastructure.Fixtures;
using EFlow.Booking.ApiTests.Infrastructure.Scenarios;
using EFlow.Booking.Domain;
using FluentAssertions;

namespace EFlow.Booking.ApiTests.Auth;

/// <summary>
/// Verifies authentication endpoints test cases.
/// </summary>
[Collection(ApiTestCollection.Name)]
public sealed class AuthApiTests(ApiTestStackFixture fixture)
{
    /// <summary>
    /// Verifies that <c>GetCurrentUser</c> returns <c>Unauthorized</c> when the caller is anonymous.
    /// </summary>
    [Fact]
    public async Task GetCurrentUser_WhenUserIsAnonymous_ShouldReturnUnauthorized()
    {
        using var session = fixture.CreateSession();

        var response = await session.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Verifies that <c>Register</c> authenticates the caller and returns the expected current-user payload when the requested role is admin.
    /// </summary>
    [Fact]
    public async Task Register_WhenRoleIsAdmin_ShouldAuthenticateUserAndReturnExpectedCurrentUser()
    {
        var scenario = new ApiScenario(fixture);
        var (adminSession, currentUser) = await scenario.CreateAdminSessionAsync();

        using (adminSession)
        {
            currentUser.Id.Should().NotBeEmpty();
            currentUser.UserName.Should().Be($"admin_{scenario.Suffix}");
            currentUser.Email.Should().Be($"admin_{scenario.Suffix}@example.com");
            currentUser.Roles.Should().Contain(Identity.Roles.Admin);
        }
    }

    /// <summary>
    /// Verifies that <c>Logout</c> invalidates the current session when the user is authenticated.
    /// </summary>
    [Fact]
    public async Task Logout_WhenUserIsAuthenticated_ShouldInvalidateSession()
    {
        var scenario = new ApiScenario(fixture);
        var (adminSession, _) = await scenario.CreateAdminSessionAsync();

        using (adminSession)
        {
            var logoutResponse = await adminSession.PostAsync<object?>("/api/auth/logout", null);

            logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            (await adminSession.ReadTextAsync(logoutResponse)).Should().Contain("Logged out successfully");

            var meAfterLogout = await adminSession.GetAsync("/api/auth/me");
            meAfterLogout.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }

    /// <summary>
    /// Verifies that <c>Register</c> returns <c>BadRequest</c> when the requested role is not admin.
    /// </summary>
    [Fact]
    public async Task Register_WhenRoleIsNotAdmin_ShouldReturnBadRequest()
    {
        var scenario = new ApiScenario(fixture);
        using var session = fixture.CreateSession();

        var response = await session.PostAsync(
            "/api/auth/register",
            new RegisterRequestModel
            {
                Username = $"student_{scenario.Suffix}",
                Password = "Student123!",
                Email = $"student_{scenario.Suffix}@example.com",
                Role = Identity.Role.Student
            });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await session.ReadTextAsync(response)).Should().Contain("Only admin registration is allowed");
    }

    /// <summary>
    /// Verifies that <c>Login</c> returns <c>Unauthorized</c> when the password is invalid.
    /// </summary>
    [Fact]
    public async Task Login_WhenPasswordIsInvalid_ShouldReturnUnauthorized()
    {
        var scenario = new ApiScenario(fixture);
        var (adminSession, _) = await scenario.CreateAdminSessionAsync();

        using (adminSession)
        {
            using var invalidLoginSession = fixture.CreateSession();

            var response = await invalidLoginSession.PostAsync(
                "/api/auth/login",
                new LoginRequestModel
                {
                    Username = $"admin_{scenario.Suffix}",
                    Password = "WrongPassword!"
                });

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            (await invalidLoginSession.ReadTextAsync(response)).Should().Contain("Invalid credentials");
        }
    }
}
