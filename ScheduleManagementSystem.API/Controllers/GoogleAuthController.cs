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
        try
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
        catch (Exception ex)
        {
            // Log the actual error to see what's happening
            Console.WriteLine($"Error in Google Login: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("callback")]
    [AllowAnonymous]
    public async Task<IActionResult> Callback([FromQuery] string returnUrl = "/")
    {
        try
        {
            // Authenticate the user
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                var errorMsg = result.Failure?.Message ?? "Google authentication failed";
                Console.WriteLine($"Google auth failed: {errorMsg}");

                // Redirect to frontend with error
                var frontendUrl = GetFrontendUrl();
                return Redirect($"{frontendUrl}/login?error={Uri.EscapeDataString(errorMsg)}");
            }

            // Extract user information from claims
            var email = result.Principal?.FindFirstValue(ClaimTypes.Email);
            var name = result.Principal?.FindFirstValue(ClaimTypes.Name);
            var googleId = result.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(googleId))
            {
                var frontendUrl = GetFrontendUrl();
                return Redirect($"{frontendUrl}/login?error={Uri.EscapeDataString("Failed to retrieve user information from Google")}");
            }

            // Use your AuthService to authenticate/create user
            var (user, token) = await _authService.AuthenticateGoogleAsync(email, googleId, name);

            // Set JWT token as HTTP-only cookie
            Response.Cookies.Append("access_token", $"Bearer {token}", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None, // Changed to None for cross-site scenarios
                MaxAge = TimeSpan.FromDays(7)
            });

            // Get return URL from properties if available
            if (result.Properties?.Items.TryGetValue("returnUrl", out var storedReturnUrl) == true)
            {
                returnUrl = storedReturnUrl;
            }

            // Redirect to frontend
            var successUrl = GetFrontendUrl();
            return Redirect($"{successUrl}{returnUrl}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Google auth error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");

            var frontendUrl = GetFrontendUrl();
            return Redirect($"{frontendUrl}/login?error={Uri.EscapeDataString(ex.Message)}");
        }
    }

    private string GetFrontendUrl()
    {
        // Simple approach - check if localhost
        if (Request.Host.Host.Contains("localhost"))
        {
            return "https://localhost:7273";
        }

        // Production - make sure this matches your actual frontend URL
        return "https://schedulemanagementsystemfullstackclient.onrender.com";
    }
}