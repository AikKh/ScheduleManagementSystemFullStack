using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScheduleManagementSystem.API.Models;
using ScheduleManagementSystem.API.Services;
using ScheduleManagementSystem.Shared.Enums;
using ScheduleManagementSystem.Shared.Models;

namespace ScheduleManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CreateController(UserService userService) : ControllerBase
{
    readonly UserService _userService = userService;

    [Authorize(Roles = "Teacher,Admin")]
    [HttpPost("teacher")]
    public async Task<IActionResult> CreateTeacher([FromBody] RegisterRequest request)
    {
        try
        {
            if (await _userService.UserExistsAsync(request.Email))
                throw new InvalidOperationException("User already exists");

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                Role = UserRole.Teacher, 
            };

            var authMethod = new AuthMethod
            {
                Provider = AuthProvider.Local,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            await _userService.CreateUserAsync(user, authMethod);

            return Ok("Teacher created successfully");
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("admin")]
    public async Task<IActionResult> CreateAdmin([FromBody] RegisterRequest request)
    {
        try
        {
            if (await _userService.UserExistsAsync(request.Email))
                throw new InvalidOperationException("User already exists");

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                Role = UserRole.Admin,
            };

            var authMethod = new AuthMethod
            {
                Provider = AuthProvider.Local,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            await _userService.CreateUserAsync(user, authMethod);

            return Ok("Admin created successfully");
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
