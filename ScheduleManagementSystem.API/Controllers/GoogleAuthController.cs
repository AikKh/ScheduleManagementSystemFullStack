using Microsoft.AspNetCore.Authentication;
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
    public IActionResult Login([FromQuery] string returnUrl = "/")
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(Callback)),
            Items =
            {
                { "returnUrl", returnUrl }
            }
        };

        return Challenge(properties, "Google");
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback()
    {
        var result = await HttpContext.AuthenticateAsync("Google");
        if (!result.Succeeded)
            return BadRequest("Google authentication failed");

        var email = result.Principal.FindFirstValue(ClaimTypes.Email);
        var name = result.Principal.FindFirstValue(ClaimTypes.Name);
        var id = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier);

        var (user, token) = await _authService.AuthenticateGoogleAsync(email, id, name);

        Response.Cookies.Append("access_token", $"Bearer {token}", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            MaxAge = TimeSpan.FromMinutes(30)
        });

        var returnUrl = result.Properties.Items["returnUrl"] ?? "/";

        return Redirect(returnUrl);
    }
}