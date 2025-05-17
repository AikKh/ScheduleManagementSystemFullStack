using System.ComponentModel.DataAnnotations;

namespace ScheduleManagementSystem.Shared.DTOs;

public class CreateUserDto
{
    [Required, StringLength(50)]
    public required string Username { get; set; }

    [Required, EmailAddress]
    public required string Email { get; set; }
}


public class UpdateUserDto
{
    [StringLength(50)]
    public string? Username { get; set; }

    [EmailAddress]
    public string? Email { get; set; } 
}

public class UserSummaryDto
{
    [Required]
    public int Id { get; set; }

    [Required]
    public required string Username { get; set; }

    [Required, EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string Role { get; set; }
}

public class UserResponseDto : UserSummaryDto
{
    [Required]
    public List<GroupSummaryDto> Groups { get; set; } = [];

    [Required]
    public List<AuthMethodSummaryDto> AuthMethods { get; set; } = [];
}
