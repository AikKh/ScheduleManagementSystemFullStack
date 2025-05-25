using System.Net.Http.Json;
using ScheduleManagementSystem.Shared.DTOs;

namespace ScheduleManagementSystem.Client.Services;

public class EventService(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<List<EventResponseDto>> GetEventsAsync()
    {
        try
        {
            var events = await _httpClient.GetFromJsonAsync<List<EventResponseDto>>("api/event");
            return events ?? [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching events: {ex.Message}");
            return [];
        }
    }

    public async Task<EventResponseDto?> GetEventByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<EventResponseDto>($"api/event/{id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching event {id}: {ex.Message}");
            return null;
        }
    }

    public async Task<EventResponseDto?> CreateEventAsync(EventCreateDto eventDto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/event", eventDto);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<EventResponseDto>();
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating event: {ex.Message}");
            return null;
        }
    }

    public async Task<EventResponseDto?> UpdateEventAsync(int id, EventUpdateDto eventDto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/event/{id}", eventDto);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<EventResponseDto>();
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating event {id}: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> DeleteEventAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/event/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting event {id}: {ex.Message}");
            return false;
        }
    }
}