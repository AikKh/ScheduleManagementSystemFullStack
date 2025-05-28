using Microsoft.AspNetCore.Mvc;
using ScheduleManagementSystem.Shared.DTOs;
using ScheduleManagementSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ScheduleManagementSystem.Shared.Models;

namespace ScheduleManagementSystem.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserController(UserService userService) : ControllerBase
{
    private readonly UserService _userService = userService;

    [HttpGet("profile")]
    public async Task<ActionResult<UserResponseDto>> GetProfile()
    {
        try
        {
            if (int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int id))
            {
                var user = await _userService.GetUserByIdAsync(id);

                if (user is null)
                {
                    return NotFound(new { Message = "User not found." });
                }

                return Ok(user);
            }
            else
            {
                return BadRequest(new { Message = "The token has no name identifier" });
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    /// <summary>
    /// Updates user profile information (username, email)
    /// </summary>
    [HttpPut("profile")]
    public async Task<ActionResult<UserResponseDto>> UpdateProfile([FromBody] UpdateUserDto updateUserDto)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (email is null)
        {
            return BadRequest("User email not found in claims.");
        }

        try
        {
            var updatedUser = await _userService.UpdateUserAsync(email, updateUserDto);
            return Ok(updatedUser);
        }
        catch (Exception ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    /// <summary>
    /// Changes user password (only available for local authentication)
    /// </summary>
    [HttpPut("password")]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (email is null)
        {
            return BadRequest("User email not found in claims.");
        }

        try
        {
            await _userService.ChangePasswordAsync(email, request.CurrentPassword, request.NewPassword);
            return Ok(new { Message = "Password changed successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    /// <summary>
    /// Deletes the current user's account permanently
    /// This action cannot be undone!
    /// </summary>
    [HttpDelete("profile")]
    public async Task<ActionResult> DeleteAccount([FromBody] DeleteAccountRequest request)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (email is null)
        {
            return BadRequest("User email not found in claims.");
        }

        try
        {
            await _userService.DeleteUserAsync(email, request.Password);

            // Clear the authentication cookie since the account is deleted
            Response.Cookies.Delete("access_token");

            return Ok(new { Message = "Account deleted successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { ex.Message });
        }
    }
}