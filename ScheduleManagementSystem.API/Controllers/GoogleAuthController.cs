using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
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
        // Build the full callback URL to ensure it's correct
        var callbackUrl = Url.Action(nameof(Callback), "GoogleAuth", new { returnUrl }, Request.Scheme);

        var properties = new AuthenticationProperties
        {
            RedirectUri = callbackUrl,
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
                Console.WriteLine($"Google auth failed: {result.Failure?.Message}");
                return RedirectToFrontend("/login", $"Google authentication failed: {result.Failure?.Message}");
            }

            // Extract user information from claims
            var email = result.Principal?.FindFirstValue(ClaimTypes.Email);
            var name = result.Principal?.FindFirstValue(ClaimTypes.Name);
            var googleId = result.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(googleId))
            {
                return RedirectToFrontend("/login", "Failed to retrieve user information from Google");
            }

            // Use your AuthService to authenticate/create user
            var (user, token) = await _authService.AuthenticateGoogleAsync(email, googleId, name);

            // Set JWT token as HTTP-only cookie
            Response.Cookies.Append("access_token", $"Bearer {token}", new CookieOptions
            {
                HttpOnly = true,
                Secure = !HttpContext.Request.Host.Host.Contains("localhost"), // Only secure in production
                SameSite = SameSiteMode.Lax,
                MaxAge = TimeSpan.FromDays(7),
                Domain = GetCookieDomain() // Set appropriate domain
            });

            // Redirect to frontend success page
            return RedirectToFrontend(returnUrl.StartsWith('/') ? returnUrl : "/");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Google auth error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");

            return RedirectToFrontend("/login", ex.Message);
        }
    }

    private IActionResult RedirectToFrontend(string path, string? error = null)
    {
        // Get the frontend URL from configuration or environment
        var frontendUrl = GetFrontendUrl();

        var redirectUrl = $"{frontendUrl}{path}";

        if (!string.IsNullOrEmpty(error))
        {
            var separator = path.Contains("?") ? "&" : "?";
            redirectUrl += $"{separator}error={Uri.EscapeDataString(error)}";
        }

        return Redirect(redirectUrl);
    }

    private string GetFrontendUrl()
    {
        if (HttpContext.Request.Host.Host.Contains("localhost"))
        {
            return "https://localhost:7273";
        }

        return "https://schedulemanagementsystemfullstackclient.onrender.com";
    }

    private string? GetCookieDomain()
    {
        if (HttpContext.Request.Host.Host.Contains("localhost"))
        {
            return null;
        }

        return null;
    }
}