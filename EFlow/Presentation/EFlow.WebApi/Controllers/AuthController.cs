using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EFlow.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace EFlow.WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    UserManager<Identity> userManager,
    SignInManager<Identity> signInManager,
    IConfiguration configuration)
    : ControllerBase
{
    // TODO: убрать этот endpoint когда появится другая возможность регистрации админов
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var user = new Identity
        {
            Id = Guid.NewGuid(),
            UserName = request.Username
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        await userManager.AddToRoleAsync(user, Identity.GetRoleName(request.Role));

        await signInManager.SignInAsync(user, true);

        var token = GenerateJwtToken(user);

        return Ok(new { Token = token });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await userManager.FindByNameAsync(request.Username);

        if (user == null)
            return Unauthorized("Invalid credentials");

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, false);

        if (!result.Succeeded)
            return Unauthorized("Invalid credentials");

        await signInManager.SignInAsync(user, true);

        var token = GenerateJwtToken(user);

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

        if (userId == null)
            return Unauthorized("User is not authenticated");

        var user = await userManager.FindByIdAsync(userId);

        if (user == null)
            return NotFound("User not found");

        var roles = await userManager.GetRolesAsync(user);

        return Ok(
            new
            {
                user.Id,
                user.UserName,
                roles
            });
    }

    private string GenerateJwtToken(Identity user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName!)
        };

        var token = new JwtSecurityToken(
            configuration["Jwt:Issuer"],
            configuration["Jwt:Audience"],
            claims,
            expires: DateTime.UtcNow.AddMinutes(configuration.GetValue<int>("Jwt:ExpireMinutes")),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public record RegisterRequest(string Username, string Password, Identity.Role Role);
}

public record LoginRequest(string Username, string Password);