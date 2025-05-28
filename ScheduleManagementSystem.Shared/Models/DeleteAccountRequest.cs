using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleManagementSystem.Shared.Models;

/// <summary>
/// Request model for deleting user account
/// </summary>
public class DeleteAccountRequest
{
    [Required]
    public required string Password { get; set; }

    [Required]
    public required bool ConfirmDelete { get; set; } = false;
}