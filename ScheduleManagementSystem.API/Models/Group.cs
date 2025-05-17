using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ScheduleManagementSystem.API.Models;

public class Group
{
    public int Id { get; set; }
    
    [Required, MaxLength(100)] 
    public required string Name { get; set; }

    [Required]
    public required string Description { get; set; }

    [JsonIgnore]
    public List<User> Users { get; set; } = [];

    [JsonIgnore]
    public List<Event> Events { get; set; } = [];
}
