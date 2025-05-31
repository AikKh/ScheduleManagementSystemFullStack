using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScheduleManagementSystem.API.Services;
using System.Security.Claims;

namespace ScheduleManagementSystem.API.Controllers;

[ApiController]
[Route("api/google_auth")]
public class GoogleAuthController(IConfiguration configuration, AuthService authService) : ControllerBase
{
    private readonly IConfiguration _configuration = configuration;
    private readonly AuthService _authService = authService;

    [HttpGet("login")]
    [AllowAnonymous]
    public IActionResult Login([FromQuery] string returnUrl = "/")
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(Callback), new { returnUrl }),
            Items =
            {
                { "returnUrl", returnUrl }
            }
        };

        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("callback")]
    [AllowAnonymous]
    public async Task<IActionResult> Callback([FromQuery] string returnUrl = "/")
    {
        try
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                return BadRequest("Google authentication failed");
            }

            // Extract user information from claims
            var email = result.Principal?.FindFirstValue(ClaimTypes.Email);
            var name = result.Principal?.FindFirstValue(ClaimTypes.Name);
            var googleId = result.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(googleId))
            {
                return BadRequest("Failed to retrieve user information from Google");
            }

            // Use your AuthService to authenticate/create user
            var (user, token) = await _authService.AuthenticateGoogleAsync(email, googleId, name);

            // CRUCIAL: Set JWT token as HTTP-only cookie
            Response.Cookies.Append("access_token", $"Bearer {token}", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                MaxAge = TimeSpan.FromDays(7) 
            });

            return Redirect(returnUrl);
        }
        catch (Exception ex)
        {
            // Log the error (add proper logging)
            Console.WriteLine($"Google auth error: {ex.Message}");

            if (Request.Headers.Accept.ToString().Contains("application/json"))
            {
                return BadRequest(new { error = ex.Message });
            }

            return Redirect($"/login?error={Uri.EscapeDataString(ex.Message)}");
        }
    }
}