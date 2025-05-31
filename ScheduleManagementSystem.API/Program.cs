using Microsoft.EntityFrameworkCore;
using ScheduleManagementSystem.API.Data;
using ScheduleManagementSystem.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp", policy =>
    {
        policy.WithOrigins("https://localhost:7273", "https://schedulemanagementsystemfullstackclient.onrender.com") // Replace with your Blazor app's URLs
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
        //.UseSnakeCaseNamingConvention()

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<GroupService>();


#region Configur Data Protection
var dataProtectionKey = builder.Configuration["DataProtection:Key"];

if (string.IsNullOrEmpty(dataProtectionKey))
{
    Console.WriteLine("CRITICAL ERROR: DataProtection__Key environment variable not found!");
    Console.WriteLine("OAuth correlation will fail. Please set DataProtection__Key in Render.com");
    throw new InvalidOperationException("DataProtection__Key environment variable is required for production");
}

try
{
    var keyBytes = Convert.FromBase64String(dataProtectionKey);
    if (keyBytes.Length < 16)
    {
        throw new InvalidOperationException("DataProtection key must be at least 128 bits (16 bytes)");
    }

    Console.WriteLine($"Data protection key loaded successfully ({keyBytes.Length * 8} bits)");

    builder.Services.AddDataProtection()
        .SetApplicationName("ScheduleManagementSystem")
        .SetDefaultKeyLifetime(TimeSpan.FromDays(90))
        .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration
        {
            EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
            ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
        });

    Console.WriteLine("Data protection configured for production");
}
catch (FormatException)
{
    Console.WriteLine("CRITICAL ERROR: DataProtection__Key is not a valid base64 string!");
    throw new InvalidOperationException("DataProtection__Key must be a valid base64-encoded string");
}
#endregion

// Local auth
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            // Map the role claim to ClaimTypes.Role
            var roleClaim = context.Principal?.FindFirst("role");
            if (roleClaim != null)
            {
                var identity = context.Principal?.Identity as ClaimsIdentity;
                identity?.AddClaim(new Claim(ClaimTypes.Role, roleClaim.Value));
            }
            return Task.CompletedTask;
        }
    };
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    // This is ONLY used during OAuth flow, not for regular API auth
    options.Cookie.Name = "ScheduleApp.OAuth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(15); // Short-lived, just for OAuth
})
.AddGoogle(googleOptions =>
{
    googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
    googleOptions.CallbackPath = "/api/google_auth/callback";

    googleOptions.Scope.Add("email");
    googleOptions.Scope.Add("profile");
    googleOptions.SaveTokens = true;

    googleOptions.CorrelationCookie.Name = "ScheduleApp.Correlation";
    googleOptions.CorrelationCookie.HttpOnly = true;
    googleOptions.CorrelationCookie.SameSite = SameSiteMode.Lax;
    googleOptions.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
    googleOptions.CorrelationCookie.IsEssential = true;
    googleOptions.CorrelationCookie.MaxAge = TimeSpan.FromMinutes(15);

    googleOptions.Events.OnRemoteFailure = context =>
    {
        Console.WriteLine($"Google OAuth failed: {context.Failure?.Message}");
        var errorMessage = context.Failure?.Message ?? "Google authentication failed";
        context.Response.Redirect($"/login?error={Uri.EscapeDataString(errorMessage)}");
        context.HandleResponse();
        return Task.CompletedTask;
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors("AllowBlazorApp");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
//app.UseStaticFiles();

app.MapControllers();

app.UseAuthentication();
app.UseAuthorization();

app.Run();