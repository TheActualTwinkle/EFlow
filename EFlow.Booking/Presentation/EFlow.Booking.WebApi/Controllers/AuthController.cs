using System.Security.Claims;
using EFlow.Booking.Domain;
using EFlow.Booking.WebApi.Contracts.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EFlow.Booking.WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    UserManager<Identity> userManager,
    SignInManager<Identity> signInManager)
    : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await userManager.FindByNameAsync(request.Username);

        if (user is null)
            return Unauthorized("Invalid credentials");

        if (await userManager.IsLockedOutAsync(user))
            return StatusCode(StatusCodes.Status423Locked, $"Account is locked until {user.LockoutEnd:O}");

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, true);

        if (result.IsLockedOut)
            return StatusCode(StatusCodes.Status423Locked, $"Account is locked until {user.LockoutEnd:O}");

        if (!result.Succeeded)
            return Unauthorized("Invalid credentials");

        await signInManager.SignInAsync(user, true);

        return Ok();
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();

        return Ok("Logged out successfully");
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(CurrentUserView), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
            return Unauthorized("User is not authenticated");

        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
            return NotFound("User not found");

        var roles = await userManager.GetRolesAsync(user);

        return Ok(
            new CurrentUserView
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Roles = roles.ToArray()
            });
    }

}
