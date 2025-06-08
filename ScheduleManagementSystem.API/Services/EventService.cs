using Microsoft.EntityFrameworkCore;
using ScheduleManagementSystem.API.Data;
using ScheduleManagementSystem.Shared.DTOs;
using ScheduleManagementSystem.API.Mapping;
using ScheduleManagementSystem.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ScheduleManagementSystem.API.Services;

public class EventService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<List<EventResponseDto>> GetGroupEvents(int groud_id)
    {
        var events = await _context.Events
            .Where(e => e.GroupId == groud_id)
            .Include(e => e.Group)
            .ToListAsync();

        return events.Select(EventMapper.ToEventResponseDto).ToList();
    }

    public async Task<List<EventResponseDto>> GetUserEvents(string email)
    {
        var user = await _context.Users
            .Include(u => u.Groups)
            .ThenInclude(g => g.Events)
            .FirstOrDefaultAsync(u => u.Email == email)
            ?? throw new KeyNotFoundException($"User with email {email} not found.");

        var eventDtos = new List<EventResponseDto>();

        foreach (var group in user.Groups)
        {
            foreach (var evt in group.Events)
            {
                eventDtos.Add(EventMapper.ToEventResponseDto(evt));
            }
        }

        return eventDtos;
    }

    private async Task<Event?> CheckEventConflictAsync(int groupId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeEventId = null)
    {
        // Get all events in the same group on the same date
        var existingEvents = await _context.Events
            .Where(e => e.GroupId == groupId &&
                       e.Date.Date == date.Date &&
                       (excludeEventId == null || e.Id != excludeEventId))
            .ToListAsync();

        foreach (var existingEvent in existingEvents)
        {
            bool hasOverlap = startTime < existingEvent.EndTime && endTime > existingEvent.StartTime;

            if (hasOverlap)
            {
                return existingEvent;
            }
        }

        return null;
    }

    public async Task<EventResponseDto> CreateEventAsync(EventCreateDto eventDto, string email)
    {
        var user = await _context.Users
            .Include(u => u.Groups)
            .ThenInclude(g => g.Events)
            .FirstOrDefaultAsync(u => u.Email == email)
            ?? throw new KeyNotFoundException($"User with email {email} not found.");

        var group = user.Groups.FirstOrDefault(g => g.Id == eventDto.GroupId)
            ?? throw new KeyNotFoundException($"Group with ID {eventDto.GroupId} not found.");

        if (eventDto.StartTime >= eventDto.EndTime)
        {
            throw new InvalidOperationException("Event start time must be before end time.");
        }

        var conflictingEvent = await CheckEventConflictAsync(
            eventDto.GroupId,
            eventDto.Date,
            eventDto.StartTime,
            eventDto.EndTime);

        if (conflictingEvent != null)
        {
            throw new InvalidOperationException(
                $"Event conflicts with existing event '{conflictingEvent.Title}' " +
                $"on {conflictingEvent.Date.ToShortDateString()} " +
                $"from {conflictingEvent.StartTime} to {conflictingEvent.EndTime}.");
        }

        var eventEntity = EventMapper.FromEventCreateDto(eventDto);
        eventEntity.Group = group;

        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        return EventMapper.ToEventResponseDto(eventEntity);
    }

    public async Task<EventResponseDto> UpdateEventAsync(int eventId, EventUpdateDto eventDto, string email)
    {
        var user = await _context.Users
            .Include(u => u.Groups)
            .FirstOrDefaultAsync(u => u.Email == email)
            ?? throw new KeyNotFoundException($"User with email {email} not found.");

        var userGroupIds = user.Groups.Select(g => g.Id).ToList();

        var eventToUpdate = await _context.Events
            .Include(e => e.Group)
            .FirstOrDefaultAsync(e => e.Id == eventId)
            ?? throw new KeyNotFoundException($"Event with ID {eventId} not found.");

        if (!userGroupIds.Contains(eventToUpdate.GroupId))
            throw new UnauthorizedAccessException($"You don't have access to update events in group {eventToUpdate.GroupId}");

        var targetGroupId = eventDto.GroupId ?? eventToUpdate.GroupId;

        if (eventDto.GroupId.HasValue && eventDto.GroupId.Value != eventToUpdate.GroupId)
        {
            if (!userGroupIds.Contains(eventDto.GroupId.Value))
                throw new UnauthorizedAccessException($"You don't have access to move events to group {eventDto.GroupId.Value}");

            var destinationGroup = await _context.Groups.FindAsync(eventDto.GroupId.Value) 
                ?? throw new KeyNotFoundException($"Destination group with ID {eventDto.GroupId.Value} not found.");
        }

        var updatedDate = eventDto.Date ?? eventToUpdate.Date;
        var updatedStartTime = eventDto.StartTime ?? eventToUpdate.StartTime;
        var updatedEndTime = eventDto.EndTime ?? eventToUpdate.EndTime;

        // Validate that start time is before end time
        if (updatedStartTime >= updatedEndTime)
        {
            throw new InvalidOperationException("Event start time must be before end time.");
        }

        var conflictingEvent = await CheckEventConflictAsync(
            targetGroupId,
            updatedDate,
            updatedStartTime,
            updatedEndTime,
            eventId);

        if (conflictingEvent != null)
        {
            var groupMessage = targetGroupId == eventToUpdate.GroupId
                ? "in the same group"
                : $"in the destination group (ID: {targetGroupId})";

            throw new InvalidOperationException(
                $"Updated event would conflict with existing event '{conflictingEvent.Title}' " +
                $"{groupMessage} on {conflictingEvent.Date.ToShortDateString()} " +
                $"from {conflictingEvent.StartTime} to {conflictingEvent.EndTime}.");
        }

        // Apply the updates
        if (eventDto.Title is not null)
            eventToUpdate.Title = eventDto.Title;

        if (eventDto.Description is not null)
            eventToUpdate.Description = eventDto.Description;

        if (eventDto.Date.HasValue)
            eventToUpdate.Date = eventDto.Date.Value;

        if (eventDto.StartTime.HasValue)
            eventToUpdate.StartTime = eventDto.StartTime.Value;

        if (eventDto.EndTime.HasValue)
            eventToUpdate.EndTime = eventDto.EndTime.Value;

        if (eventDto.Type.HasValue)
            eventToUpdate.Type = eventDto.Type.Value;

        if (eventDto.GroupId.HasValue)
            eventToUpdate.GroupId = eventDto.GroupId.Value;

        _context.Events.Update(eventToUpdate);
        await _context.SaveChangesAsync();

        var updatedEventWithGroup = await _context.Events
            .Include(e => e.Group)
            .FirstAsync(e => e.Id == eventId);

        return EventMapper.ToEventResponseDto(updatedEventWithGroup);
    }

    public async Task<bool> DeleteEventAsync(int eventId, string email)
    {
        var user = await _context.Users
            .Include(u => u.Groups)
            .FirstOrDefaultAsync(u => u.Email == email)
            ?? throw new KeyNotFoundException($"User with email {email} not found.");

        var userGroupIds = user.Groups.Select(g => g.Id).ToList();

        var eventToDelete = await _context.Events
            .FirstOrDefaultAsync(e => e.Id == eventId)
            ?? throw new KeyNotFoundException($"Event with ID {eventId} not found.");

        if (!userGroupIds.Contains(eventToDelete.GroupId))
            throw new UnauthorizedAccessException($"You don't have access to delete events in group {eventToDelete.GroupId}");

        _context.Events.Remove(eventToDelete);
        await _context.SaveChangesAsync();

        return true;
    }
}