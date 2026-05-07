using System.Security.Claims;
using EFlow.Booking.Domain;
using EFlow.Booking.WebApi.Contracts.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EFlow.Booking.WebApi.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = $"{Identity.Roles.Admin},{Identity.Roles.Teacher},{Identity.Roles.Student}")]
public sealed class UsersController(UserManager<Identity> userManager) : ControllerBase
{
    [HttpPatch("{id:guid}/email")]
    public async Task<IActionResult> UpdateEmail(Guid id, [FromBody] UpdateUserEmailRequest request)
    {
        if (!CanManageUser(id))
            return ForbiddenOwnAccountOnly();

        var user = await userManager.FindByIdAsync(id.ToString());

        if (user is null)
            return NotFound("User not found");

        var email = request.Email.Trim();
        var result = await userManager.SetEmailAsync(user, email);

        return result.Succeeded ? NoContent() : IdentityProblem(result);
    }

    [HttpPatch("{id:guid}/password")]
    public async Task<IActionResult> UpdatePassword(Guid id, [FromBody] UpdateUserPasswordRequest request)
    {
        if (!CanManageUser(id))
            return ForbiddenOwnAccountOnly();

        var user = await userManager.FindByIdAsync(id.ToString());

        if (user is null)
            return NotFound("User not found");

        IdentityResult result;

        if (User.IsInRole(Identity.Roles.Admin))
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var admin = adminId is null ? null : await userManager.FindByIdAsync(adminId);

            if (admin is null)
                return Unauthorized("User is not authenticated");

            if (!await userManager.CheckPasswordAsync(admin, request.CurrentPassword))
                return BadRequest(
                    new ValidationProblemDetails(
                        new Dictionary<string, string[]>
                        {
                            [nameof(request.CurrentPassword)] = ["Current password is invalid"]
                        }));

            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            
            result = await userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);
        }
        else
            result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        return result.Succeeded ? NoContent() : IdentityProblem(result);
    }

    private bool CanManageUser(Guid id)
    {
        if (User.IsInRole(Identity.Roles.Admin))
            return true;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return string.Equals(userId, id.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private IActionResult ForbiddenOwnAccountOnly() =>
        Problem(
            title: "Forbidden",
            detail: "You can only update your own account.",
            statusCode: StatusCodes.Status403Forbidden);

    private IActionResult IdentityProblem(IdentityResult result) =>
        BadRequest(
            new ValidationProblemDetails(
                result.Errors
                    .GroupBy(error => error.Code)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.Description).ToArray())));
}
