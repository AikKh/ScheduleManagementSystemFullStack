using System.Net.Http.Json;
using ScheduleManagementSystem.Shared.DTOs;

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
}
