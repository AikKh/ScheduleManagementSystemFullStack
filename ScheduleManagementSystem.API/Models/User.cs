using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ScheduleManagementSystem.Shared.Enums;

namespace ScheduleManagementSystem.API.Models;

public class User
{
    public int Id { get; set; }
    
    [Required, MaxLength(50)]
    public required string Username { get; set; }

    [EmailAddress] 
    public required string Email { get; set; }

    [Required]
    public required UserRole Role { get; set; } = UserRole.Student;

    public List<Group> Groups { get; set; } = [];
    
    public List<AuthMethod> AuthMethods { get; set; } = [];
}