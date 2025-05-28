using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleManagementSystem.Shared.Models;

/// <summary>
/// Request model for changing user password
/// </summary>
public class ChangePasswordRequest
{
    [Required]
    public required string CurrentPassword { get; set; }

    [Required]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    public required string NewPassword { get; set; }

    [Required]
    [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
    public required string ConfirmNewPassword { get; set; }
}
