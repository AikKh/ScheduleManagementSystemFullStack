using System.ComponentModel.DataAnnotations;

namespace ScheduleManagementSystem.Shared.DTOs;

public class GroupSummaryDto
{
    [Required]
    public int Id { get; set; }

    [Required]
    public required string Name { get; set; }

    [Required]
    public required string Description { get; set; }
}

public class GroupResponseDto : GroupSummaryDto
{
    public List<UserSummaryDto> Users { get; set; } = [];

    public List<EventSummaryDto> Events { get; set; } = [];
}
