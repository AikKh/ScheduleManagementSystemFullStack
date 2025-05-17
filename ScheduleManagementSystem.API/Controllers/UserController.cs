using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using ScheduleManagementSystem.API.Data;
using ScheduleManagementSystem.Shared.DTOs;
using ScheduleManagementSystem.API.Models;
using ScheduleManagementSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Data;
using System.IdentityModel.Tokens.Jwt;

namespace ScheduleManagementSystem.API.Controllers;

//[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserController(UserService userService) : ControllerBase
{
    private readonly UserService _userService = userService;

    [Authorize(Roles = "Student")]
    [HttpGet("profile")]
    public async Task<ActionResult<UserResponseDto>> GetProfile()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;

        if (email is not null)
        {
            var user = await _userService.GetUserByEmailAsync(email);

            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            return Ok(user);
        }
        else
        {
            return BadRequest("User email not found in claims.");
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserSummaryDto>>> GetUsers()
    {
        var users = await _userService.GetAllUsersAsync();

        return users == null
            ? NotFound()
            : Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponseDto>> GetUser(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);

        return user == null
            ? NotFound()
            : Ok(user);
    }

    //[HttpPost]
    //public async Task<ActionResult<CourseDto>> CreateCourse(CreateCourseDto courseDto)
    //{
    //    var createdCourse = await _courseService.CreateCourseAsync(courseDto);
    //    return CreatedAtAction(nameof(GetCourse), new { id = createdCourse.Id }, createdCourse);
    //}

    //[HttpPut("{id}")]
    //public async Task<IActionResult> UpdateCourse(int id, UpdateCourseDto courseDto)
    //{
    //    var updatedCourse = await _courseService.UpdateCourseAsync(id, courseDto);
    //    if (updatedCourse == null)
    //        return NotFound();

    //    return Ok(updatedCourse);
    //}

    //[HttpDelete("{id}")]
    //public async Task<IActionResult> DeleteCourse(int id)
    //{
    //    var result = await _courseService.DeleteCourseAsync(id);
    //    if (!result)
    //        return NotFound();

    //    return NoContent();
    //}
}