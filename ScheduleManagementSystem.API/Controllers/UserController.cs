using Microsoft.AspNetCore.Mvc;
using ScheduleManagementSystem.Shared.DTOs;
using ScheduleManagementSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ScheduleManagementSystem.API.Controllers;

//[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserController(UserService userService) : ControllerBase
{
    private readonly UserService _userService = userService;

    [Authorize(Roles = "Student,Teacher")]
    [HttpGet("profile")]
    public async Task<ActionResult<UserResponseDto>> GetProfile()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (email is null)
        {
            return BadRequest("User email not found in claims.");
        }

        try
        {
            var user = await _userService.GetUserByEmailAsync(email);
            if (user is null)
            {
                return NotFound(new { Message = "User not found." });
            }
            return Ok(user);
        }
        catch (Exception ex)
        {
            return BadRequest(new { ex.Message });
        }
    }
}