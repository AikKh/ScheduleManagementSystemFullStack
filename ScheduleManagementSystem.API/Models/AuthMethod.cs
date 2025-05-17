using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ScheduleManagementSystem.Shared.Enums;

namespace ScheduleManagementSystem.API.Models;

public class AuthMethod
{
    public int Id { get; set; }
    
    [Required]
    public AuthProvider Provider { get; set; }
    
    public string? ProviderUserId { get; set; }
    
    public string? PasswordHash { get; set; }

    [Required]
    public int UserId { get; set; }

    [JsonIgnore]
    public User? User { get; set; }
}
