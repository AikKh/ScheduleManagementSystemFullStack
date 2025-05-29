using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScheduleManagementSystem.API.Models;
using ScheduleManagementSystem.API.Services;
using ScheduleManagementSystem.Shared.Enums;
using ScheduleManagementSystem.Shared.Models;

namespace ScheduleManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AuthService authService, UserService userService, JwtService jwtService) : ControllerBase
{
    private readonly AuthService _authService = authService;
    private readonly UserService _userService = userService;
    private readonly JwtService _jwtService = jwtService;

    [HttpPost("login"), AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var (_, token) = await _authService.AuthenticateLocalAsync(request.Email, request.Password);

            // Set the token as a cookie
            Response.Cookies.Append("access_token", $"Bearer {token}", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                MaxAge = TimeSpan.FromDays(7)
            });

            return Ok(new { token });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("register"), AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // We already check this in RegisterRequest, but also here just in case
            if (request.Password != request.ConfirmPassword)
                throw new InvalidOperationException("Passwords do not match");

            if (await _userService.UserExistsAsync(request.Email))
                throw new InvalidOperationException("User already exists");

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                Role = UserRole.Student, // Default role
            };

            var authMethod = new AuthMethod
            {
                Provider = AuthProvider.Local,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            await _userService.CreateUserAsync(user, authMethod);

            var token = _jwtService.GenerateJwtToken(user);

            Response.Cookies.Append("access_token", $"Bearer {token}", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                MaxAge = TimeSpan.FromDays(7)
            });

            return Ok(new { token });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("access_token");
        return Ok();
    }
}