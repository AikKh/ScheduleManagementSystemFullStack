using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ScheduleManagementSystem.Shared.Enums;

namespace ScheduleManagementSystem.API.Models;

public class Event
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public required string Title { get; set; }

    public string? Description { get; set; }

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

    [ForeignKey("GroupId")]
    public Group Group { get; set; }
}