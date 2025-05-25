using System;
using System.ComponentModel.DataAnnotations;
using ScheduleManagementSystem.Shared.Enums;

namespace ScheduleManagementSystem.Shared.DTOs;

public class EventCreateDto
{
    [Required, StringLength(100)]
    public required string Title { get; set; }

    public string Description { get; set; } = "";

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    [Required]
    public EventType Type { get; set; }

    [Required]
    public int GroupId { get; set; }
}

public class EventUpdateDto
{
    [StringLength(100)]
    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateTime? Date { get; set; }

    public TimeSpan? StartTime { get; set; }

    public TimeSpan? EndTime { get; set; }

    public EventType? Type { get; set; }
}

public class EventSummaryDto : EventCreateDto
{
    [Required]
    public int Id { get; set; }
}

public class EventResponseDto : EventSummaryDto
{
    // Group information
    public GroupSummaryDto Group { get; set; }
}