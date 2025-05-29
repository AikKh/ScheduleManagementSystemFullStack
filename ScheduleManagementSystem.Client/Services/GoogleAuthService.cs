using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using ScheduleManagementSystem.Client.Providers;
using System.Text.Json;

namespace ScheduleManagementSystem.Client.Services
{
    public class GoogleAuthService(
        NavigationManager navigationManager,
        HttpClient httpClient,
        ILocalStorageService localStorageService,
        AuthenticationStateProvider authStateProvider,
        IJSRuntime jsRuntime)
    {
        private readonly NavigationManager _navigationManager = navigationManager;
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILocalStorageService _localStorageService = localStorageService;
        private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;
        private readonly IJSRuntime _jsRuntime = jsRuntime;

        /// <summary>
        /// Initiates the Google OAuth login flow by redirecting to the backend Google auth endpoint
        /// </summary>
        /// <param name="returnUrl">URL to redirect to after successful authentication</param>
        public async Task InitiateGoogleLoginAsync(string? returnUrl = null)
        {
            try
            {
                // Default return URL
                var redirectUrl = returnUrl ?? "/profile";

                // Store the intended return URL in local storage for later retrieval
                await _localStorageService.SetItem("google_auth_return_url", redirectUrl);

                // Build the Google auth URL with return URL
                var googleAuthUrl = $"/api/google_auth/login?returnUrl={Uri.EscapeDataString(redirectUrl)}";

                // Navigate to Google OAuth (this will redirect to Google, then back to our callback)
                _navigationManager.NavigateTo(googleAuthUrl, forceLoad: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initiating Google login: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Handles the Google OAuth callback and processes the authentication result
        /// </summary>
        /// <returns>True if authentication was successful, false otherwise</returns>
        public async Task<bool> HandleGoogleCallbackAsync()
        {
            try
            {
                var uri = _navigationManager.ToAbsoluteUri(_navigationManager.Uri);
                var queryParams = ParseQueryString(uri.Query);

                // Check if we have a success parameter or token in the URL
                if (queryParams.ContainsKey("success") && queryParams["success"] == "true")
                {
                    // If the backend has already set the token in a cookie or we need to fetch it
                    return await ProcessSuccessfulAuthenticationAsync();
                }
                else if (queryParams.ContainsKey("token"))
                {
                    // If the token is passed directly in the URL
                    var token = queryParams["token"];
                    return await ProcessTokenAsync(token);
                }
                else if (queryParams.ContainsKey("error"))
                {
                    // Handle authentication error
                    var error = queryParams["error"];
                    Console.WriteLine($"Google authentication failed: {error}");
                    return false;
                }

                // Check if we're on the callback URL without explicit success/error parameters
                // In this case, try to fetch the current auth state
                return await CheckAndProcessAuthStateAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling Google callback: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks the current authentication state and processes any pending Google auth
        /// </summary>
        public async Task<GoogleAuthState> CheckAuthStateAsync()
        {
            try
            {
                var authState = await _authStateProvider.GetAuthenticationStateAsync();

                if (authState.User.Identity?.IsAuthenticated == true)
                {
                    // Get the stored return URL
                    var returnUrl = await _localStorageService.GetItem<string>("google_auth_return_url");

                    return new GoogleAuthState
                    {
                        IsAuthenticated = true,
                        ReturnUrl = returnUrl ?? "/profile",
                        Error = null
                    };
                }

                return new GoogleAuthState
                {
                    IsAuthenticated = false,
                    ReturnUrl = null,
                    Error = null
                };
            }
            catch (Exception ex)
            {
                return new GoogleAuthState
                {
                    IsAuthenticated = false,
                    ReturnUrl = null,
                    Error = ex.Message
                };
            }
        }

        private async Task<bool> ProcessSuccessfulAuthenticationAsync()
        {
            try
            {
                // The backend should have already set the authentication cookie/token
                // Force refresh the authentication state
                if (_authStateProvider is CustomAuthStateProvider customProvider)
                {
                    // Check if we have a token in local storage (set by the backend callback)
                    var token = await _localStorageService.GetItem<string>("authToken");

                    if (!string.IsNullOrEmpty(token))
                    {
                        customProvider.NotifyUserAuthentication(token);
                        return true;
                    }
                }

                // Alternative: Make a request to check current auth status
                return await ValidateAuthenticationWithBackendAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing successful authentication: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> ProcessTokenAsync(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return false;

                // Store the token
                await _localStorageService.SetItem("authToken", token);

                // Notify the auth state provider
                if (_authStateProvider is CustomAuthStateProvider customProvider)
                {
                    customProvider.NotifyUserAuthentication(token);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing token: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> CheckAndProcessAuthStateAsync()
        {
            try
            {
                // Make a request to the backend to check if we're authenticated
                var response = await _httpClient.GetAsync("/api/user/profile");

                if (response.IsSuccessStatusCode)
                {
                    // We're authenticated, check if we have a token
                    var token = await _localStorageService.GetItem<string>("authToken");

                    if (string.IsNullOrEmpty(token))
                    {
                        // Try to get token from the authentication check endpoint
                        return await RequestTokenFromBackendAsync();
                    }

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking auth state: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> ValidateAuthenticationWithBackendAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/user/profile");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> RequestTokenFromBackendAsync()
        {
            try
            {
                // This endpoint should return the current user's JWT token
                var response = await _httpClient.GetAsync("/api/auth/token");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);

                    if (doc.RootElement.TryGetProperty("token", out var tokenElement))
                    {
                        var token = tokenElement.GetString();
                        if (!string.IsNullOrEmpty(token))
                        {
                            await _localStorageService.SetItem("authToken", token);

                            if (_authStateProvider is CustomAuthStateProvider customProvider)
                            {
                                customProvider.NotifyUserAuthentication(token);
                            }

                            return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error requesting token from backend: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Cleans up any Google auth-related storage after successful authentication
        /// </summary>
        public async Task CleanupAuthStateAsync()
        {
            try
            {
                await _localStorageService.RemoveItem("google_auth_return_url");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cleaning up auth state: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the stored return URL for post-authentication redirect
        /// </summary>
        public async Task<string?> GetStoredReturnUrlAsync()
        {
            try
            {
                return await _localStorageService.GetItem<string>("google_auth_return_url");
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Parses a query string into a dictionary of key-value pairs
        /// </summary>
        /// <param name="queryString">The query string to parse (including the leading ?)</param>
        /// <returns>Dictionary of query parameters</returns>
        private static Dictionary<string, string> ParseQueryString(string queryString)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrEmpty(queryString))
                return result;

            // Remove leading ? if present
            if (queryString.StartsWith("?"))
                queryString = queryString.Substring(1);

            if (string.IsNullOrEmpty(queryString))
                return result;

            var pairs = queryString.Split('&');

            foreach (var pair in pairs)
            {
                if (string.IsNullOrEmpty(pair))
                    continue;

                var keyValue = pair.Split('=', 2);
                if (keyValue.Length >= 1)
                {
                    var key = Uri.UnescapeDataString(keyValue[0]);
                    var value = keyValue.Length == 2 ? Uri.UnescapeDataString(keyValue[1]) : "";

                    // Use the last value if key appears multiple times
                    result[key] = value;
                }
            }

            return result;
        }
    }

    /// <summary>
    /// Represents the state of Google authentication
    /// </summary>
    public class GoogleAuthState
    {
        public bool IsAuthenticated { get; set; }
        public string? ReturnUrl { get; set; }
        public string? Error { get; set; }
    }
}