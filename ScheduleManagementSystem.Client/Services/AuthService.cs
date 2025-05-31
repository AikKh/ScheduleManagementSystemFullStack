using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using Microsoft.JSInterop;
using ScheduleManagementSystem.Shared.Models;
using ScheduleManagementSystem.Shared.DTOs;
using Microsoft.AspNetCore.Components.Authorization;
using ScheduleManagementSystem.Client.Providers;
using Microsoft.AspNetCore.Components;

namespace ScheduleManagementSystem.Client.Services;

public class AuthService(
    HttpClient httpClient,
    ILocalStorageService localStorageService,
    AuthenticationStateProvider authStateProvider,
    NavigationManager navigationManager)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILocalStorageService _localStorageService = localStorageService;
    private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;
    private readonly NavigationManager _navigationManager = navigationManager;

    public async Task<bool> LoginAsync(LoginRequest loginRequest)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginRequest);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(json);
                var token = doc.RootElement.GetProperty("token").GetString();

                await _localStorageService.SetItem("authToken", token);

                // Notify auth state has changed
                ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(token);

                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Login error: " + ex.Message);
            return false;
        }
    }

    public async Task<bool> RegisterAsync(RegisterRequest registerRequest)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", registerRequest);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(json);
                var token = doc.RootElement.GetProperty("token").GetString();

                await _localStorageService.SetItem("authToken", token);

                // Notify auth state has changed
                ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(token);

                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        await _httpClient.GetAsync("api/auth/logout");
        await _localStorageService.RemoveItem("authToken");
        ((CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await _localStorageService.GetItem<string>("authToken");
        return !string.IsNullOrEmpty(token);
    }

    public void LoginWithGoogle(string returnUrl = "/profile")
    {
        try
        {
            // Get the current Blazor app URL to return to after authentication
            var currentUri = _navigationManager.ToAbsoluteUri(_navigationManager.Uri);
            var blazorBaseUrl = $"{currentUri.Scheme}://{currentUri.Authority}";

            // The return URL should be back to the Blazor app, not the API
            var fullReturnUrl = $"{blazorBaseUrl}{returnUrl}";
            var encodedReturnUrl = Uri.EscapeDataString(fullReturnUrl);

            // CRUCIAL: Navigate to the API server, not the client server
            var googleLoginUrl = $"{_httpClient.BaseAddress}api/google_auth/login?returnUrl={encodedReturnUrl}";

            Console.WriteLine($"Navigating to Google auth: {googleLoginUrl}");
            Console.WriteLine($"Return URL will be: {fullReturnUrl}");

            // Navigate to the API server for Google authentication
            _navigationManager.NavigateTo(googleLoginUrl, forceLoad: true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initiating Google login: {ex.Message}");
            throw new Exception($"Failed to start Google authentication: {ex.Message}");
        }
    }

}