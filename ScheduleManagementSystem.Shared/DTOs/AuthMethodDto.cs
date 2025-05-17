using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScheduleManagementSystem.Shared.Enums;

namespace ScheduleManagementSystem.Shared.DTOs;

public class AuthMethodSummaryDto
{
    [Required]
    public required int Id { get; set; }

    [Required]
    public required int UserId { get; set; }
    
    [Required]
    public required AuthProvider Provider { get; set; }
    

}
public class AuthMethodResponseDto : AuthMethodSummaryDto
{
    public string? ProviderUserId { get; set; } // For OAuth providers
    
    public string? PasswordHash { get; set; }

    public UserSummaryDto? User { get; set; }
}

public class AuthMethodUpdateDto
{
    [Required]
    public required string PasswordHash { get; set; }
}