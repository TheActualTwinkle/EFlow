using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EFlow.Booking.Domain;
using EFlow.Booking.WebApi.Contracts.Auth;
using EFlow.Common.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace EFlow.Booking.WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    UserManager<Identity> userManager,
    SignInManager<Identity> signInManager,
    IConfiguration configuration,
    ISystemClock systemClock)
    : ControllerBase
{
    // TODO: убрать этот endpoint когда появится другая возможность регистрации админов из файлика
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (request.Role is not Identity.Role.Admin)
            return BadRequest("Only admin registration is allowed");

        var user = new Identity
        {
            Id = Guid.NewGuid(),
            UserName = request.Username,
            Email = request.Email
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        await userManager.AddToRoleAsync(user, Identity.GetRoleName(request.Role));

        await signInManager.SignInAsync(user, true);

        var token = await GenerateJwtTokenAsync(user);

        return Ok(new { Token = token });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await userManager.FindByNameAsync(request.Username);

        if (user is null)
            return Unauthorized("Invalid credentials");

        if (await userManager.IsLockedOutAsync(user))
            return Unauthorized($"Account is locked until {user.LockoutEnd:O}");

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, true);

        if (result.IsLockedOut)
            return Unauthorized($"Account is locked until {user.LockoutEnd:O}");

        if (!result.Succeeded)
            return Unauthorized("Invalid credentials");

        await signInManager.SignInAsync(user, true);

        var token = await GenerateJwtTokenAsync(user);

        return Ok(new { Token = token });
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
            new
            {
                user.Id,
                user.UserName,
                user.Email,
                roles
            });
    }

    private async Task<string> GenerateJwtTokenAsync(Identity user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var roles = await userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.Email, user.Email ?? string.Empty)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            configuration["Jwt:Issuer"],
            configuration["Jwt:Audience"],
            claims,
            expires: systemClock.UtcNow.AddMinutes(configuration.GetValue<int>("Jwt:ExpireMinutes")),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
