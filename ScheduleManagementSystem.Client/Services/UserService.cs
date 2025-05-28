using System.Net.Http.Json;
using ScheduleManagementSystem.Shared.DTOs;
using ScheduleManagementSystem.Shared.Models;

namespace ScheduleManagementSystem.Client.Services;

public class UserService(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<UserResponseDto> GetCurrentUserAsync()
    {
        try
        {
            var user = await _httpClient.GetFromJsonAsync<UserResponseDto>("api/user/profile");

            if (user == null)
            {
                return null;
            }
            return user;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error fetching current user: " + ex.Message);
            return null;
        }
    }

    public async Task<UserResponseDto> GetUserByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<UserResponseDto>($"api/user/{id}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> UpdateProfileAsync(UpdateUserDto updateDto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync("api/user/profile", updateDto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error updating profile: " + ex.Message);
            return false;
        }
    }

    public async Task<bool> ChangePasswordAsync(ChangePasswordRequest passwordRequest)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync("api/user/password", passwordRequest);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error changing password: " + ex.Message);
            return false;
        }
    }

    public async Task<bool> DeleteAccountAsync(DeleteAccountRequest deleteRequest)
    {
        try
        {
            var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "api/user/profile")
            {
                Content = JsonContent.Create(deleteRequest)
            });
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error deleting account: " + ex.Message);
            return false;
        }
    }

    public async Task<bool> CreateAdminAsync(RegisterRequest registerRequest)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/create/admin", registerRequest);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Failed to create admin: {response.StatusCode} - {errorContent}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error creating admin: " + ex.Message);
            return false;
        }
    }

    public async Task<bool> CreateTeacherAsync(RegisterRequest registerRequest)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/create/teacher", registerRequest);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Failed to create teacher: {response.StatusCode} - {errorContent}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error creating teacher: " + ex.Message);
            return false;
        }
    }
}