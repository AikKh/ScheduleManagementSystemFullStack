using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using ScheduleManagementSystem.Client;
using ScheduleManagementSystem.Client.Providers;
using ScheduleManagementSystem.Client.Services;
using System;
using System.Net.Http;
using System.Text.RegularExpressions;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

//var apiAddress = Environment.GetEnvironmentVariable("API_URL") ?? "https://localhost:7189/";
var apiAddress = "https://schedulemanagementsystemfullstack.onrender.com";

if (builder.HostEnvironment.IsDevelopment())
{
    apiAddress = "https://localhost:7189/";
}

builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddScoped<BearerTokenHandler>();

builder.Services.AddScoped(sp => {
    var bearerTokenHandler = sp.GetRequiredService<BearerTokenHandler>();
    var innerHandler = new HttpClientHandler();

    bearerTokenHandler.InnerHandler = innerHandler;

    var httpClient = new HttpClient(bearerTokenHandler)
    {
        BaseAddress = new Uri(apiAddress)
    };

    return httpClient;
});

// Add authentication state provider
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddAuthorizationCore();

// Register services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<GroupService>();
builder.Services.AddScoped<GoogleAuthService>();

await builder.Build().RunAsync();