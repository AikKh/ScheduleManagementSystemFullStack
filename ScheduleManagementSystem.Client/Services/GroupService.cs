using System.Net.Http.Json;
using ScheduleManagementSystem.Shared.DTOs;

namespace ScheduleManagementSystem.Client.Services;

public class GroupService(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<List<GroupSummaryDto>> GetUserGroupsAsync()
    {
        try
        {
            var groups = await _httpClient.GetFromJsonAsync<List<GroupSummaryDto>>("api/group");
            return groups ?? new List<GroupSummaryDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching user groups: {ex.Message}");
            return new List<GroupSummaryDto>();
        }
    }

    public async Task<List<GroupSummaryDto>> GetAllGroupsAsync()
    {
        try
        {
            var groups = await _httpClient.GetFromJsonAsync<List<GroupSummaryDto>>("api/group/all");
            return groups ?? new List<GroupSummaryDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching all groups: {ex.Message}");
            return new List<GroupSummaryDto>();
        }
    }

    public async Task<GroupResponseDto?> GetGroupByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<GroupResponseDto>($"api/group/{id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching group {id}: {ex.Message}");
            return null;
        }
    }

    public async Task<List<GroupSummaryDto>> SearchGroupsAsync(string name)
    {
        try
        {
            var groups = await _httpClient.GetFromJsonAsync<List<GroupSummaryDto>>($"api/group/search?name={Uri.EscapeDataString(name)}");
            return groups ?? new List<GroupSummaryDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching groups: {ex.Message}");
            return new List<GroupSummaryDto>();
        }
    }

    public async Task<GroupResponseDto?> CreateGroupAsync(GroupCreateDto groupDto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/group", groupDto);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<GroupResponseDto>();
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating group: {ex.Message}");
            return null;
        }
    }

    public async Task<GroupResponseDto?> UpdateGroupAsync(int id, GroupUpdateDto groupDto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/group/{id}", groupDto);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<GroupResponseDto>();
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating group {id}: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> DeleteGroupAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/group/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting group {id}: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> JoinGroupAsync(int groupId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/group/{groupId}/join", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error joining group {groupId}: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> LeaveGroupAsync(int groupId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/group/{groupId}/leave", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error leaving group {groupId}: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> AddUserToGroupAsync(int groupId, int userId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/group/{groupId}/users/{userId}", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding user {userId} to group {groupId}: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> RemoveUserFromGroupAsync(int groupId, int userId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/group/{groupId}/users/{userId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing user {userId} from group {groupId}: {ex.Message}");
            return false;
        }
    }
}