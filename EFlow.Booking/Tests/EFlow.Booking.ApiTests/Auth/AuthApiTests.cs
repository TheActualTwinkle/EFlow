using System.Net;
using EFlow.Booking.ApiTests.Infrastructure.Collections;
using EFlow.Booking.ApiTests.Infrastructure.Contracts;
using EFlow.Booking.ApiTests.Infrastructure.Fixtures;
using EFlow.Booking.ApiTests.Infrastructure.Scenarios;
using FluentAssertions;

namespace EFlow.Booking.ApiTests.Auth;

/// <summary>
/// Verifies authentication endpoints test cases.
/// </summary>
[Collection(ApiTestCollection.Name)]
public sealed class AuthApiTests(ApiTestStackFixture fixture)
{
    private const string InvalidPassword = "WrongPassword!";
    private const string InvalidCredentialsMessage = "Invalid credentials";

    /// <summary>
    /// Verifies that <c>GetCurrentUser</c> returns <c>Unauthorized</c> when the caller is anonymous.
    /// </summary>
    [Fact]
    public async Task GetCurrentUser_WhenUserIsAnonymous_ShouldReturnUnauthorized()
    {
        // Arrange
        using var session = fixture.CreateSession();

        // Act
        var response = await session.GetAsync(ApiScenario.CurrentUserPath);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Verifies that <c>Login</c> authenticates the caller and returns the expected current-user payload for the configured administrator.
    /// </summary>
    [Fact]
    public async Task Login_WhenCredentialsAreValid_ShouldAuthenticateUserAndReturnExpectedCurrentUser()
    {
        // Arrange
        var scenario = new ApiScenario(fixture);
        var (adminSession, currentUser) = await scenario.CreateAdminSessionAsync();

        using (adminSession)
        {
            // Act & Assert
            currentUser.Id.Should().NotBeEmpty();
            currentUser.UserName.Should().Be(fixture.AdminUsername);
            currentUser.Email.Should().Be(fixture.AdminEmail);
            currentUser.Roles.Should().Contain("Admin");
        }
    }

    /// <summary>
    /// Verifies that <c>Logout</c> invalidates the current session when the user is authenticated.
    /// </summary>
    [Fact]
    public async Task Logout_WhenUserIsAuthenticated_ShouldInvalidateSession()
    {
        // Arrange
        var scenario = new ApiScenario(fixture);
        var (adminSession, _) = await scenario.CreateAdminSessionAsync();

        using (adminSession)
        {
            // Act
            var logoutResponse = await adminSession.PostAsync<object?>("/api/auth/logout", null);
            var meAfterLogout = await adminSession.GetAsync(ApiScenario.CurrentUserPath);

            // Assert
            logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            (await adminSession.ReadTextAsync(logoutResponse)).Should().Contain("Logged out successfully");
            meAfterLogout.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }

    /// <summary>
    /// Verifies that <c>Login</c> returns <c>Unauthorized</c> when the username does not exist.
    /// </summary>
    [Fact]
    public async Task Login_WhenUserDoesNotExist_ShouldReturnUnauthorized()
    {
        // Arrange
        using var session = fixture.CreateSession();

        // Act
        var response = await session.PostAsync(
            ApiScenario.LoginPath,
            new LoginRequestModel
            {
                Username = "missing-user",
                Password = InvalidPassword
            });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await session.ReadTextAsync(response)).Should().Contain(InvalidCredentialsMessage);
    }

    /// <summary>
    /// Verifies that <c>Login</c> returns <c>Unauthorized</c> when the password is invalid.
    /// </summary>
    [Fact]
    public async Task Login_WhenPasswordIsInvalid_ShouldReturnUnauthorized()
    {
        // Arrange
        var scenario = new ApiScenario(fixture);
        var (adminSession, _) = await scenario.CreateAdminSessionAsync();

        using (adminSession)
        {
            using var invalidLoginSession = fixture.CreateSession();

            // Act
            var response = await invalidLoginSession.PostAsync(
                ApiScenario.LoginPath,
                new LoginRequestModel
                {
                    Username = fixture.AdminUsername,
                    Password = InvalidPassword
                });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            (await invalidLoginSession.ReadTextAsync(response)).Should().Contain(InvalidCredentialsMessage);
        }
    }
}
