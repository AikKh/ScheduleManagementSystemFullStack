using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using ScheduleManagementSystem.Client;
using ScheduleManagementSystem.Client.Providers;
using ScheduleManagementSystem.Client.Services;
using System;
using System.Net.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

//var apiAddress = Environment.GetEnvironmentVariable("API_URL") ?? "https://localhost:7189/";
var apiAddress = "https://schedulemanagementsystemfullstack.onrender.com";

// Register LocalStorageService first since BearerTokenHandler depends on it
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();

// Register the BearerTokenHandler
builder.Services.AddScoped<BearerTokenHandler>();

// Register HttpClient with the BearerTokenHandler
builder.Services.AddScoped(sp => {
    // Get the handler from the service provider
    var bearerTokenHandler = sp.GetRequiredService<BearerTokenHandler>();

    // Create a standard HttpClientHandler as the inner handler
    var innerHandler = new HttpClientHandler();

    // Set the inner handler
    bearerTokenHandler.InnerHandler = innerHandler;

    // Create and configure the HttpClient with the handler chain
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

await builder.Build().RunAsync();