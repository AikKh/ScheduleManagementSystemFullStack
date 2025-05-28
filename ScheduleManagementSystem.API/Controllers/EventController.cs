using Microsoft.AspNetCore.Mvc;
using ScheduleManagementSystem.Shared.DTOs;
using ScheduleManagementSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ScheduleManagementSystem.API.Controllers;

//[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EventController(EventService eventService) : ControllerBase
{
    private readonly EventService _eventService = eventService;

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<EventResponseDto>>> GetEvents()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (email is null)
        {
            return BadRequest("User email not found in claims.");
        }

        try
        {
            var events = await _eventService.GetUserEvents(email);
            if (events is null)
            {
                return NotFound(new { Message = "Events not found." });
            }
            return Ok(events);
        }
        catch (Exception ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    [Authorize(Roles = "Teacher,Admin")]
    [HttpPost]
    public async Task<ActionResult<EventResponseDto>> CreateEvent([FromBody] EventCreateDto eventDto)
    {
        try
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            if (email is null)
            {
                return BadRequest("User email not found in claims.");
            }

            var createdEvent = await _eventService.CreateEventAsync(eventDto, email);
            return CreatedAtAction(
                nameof(GetEventById),
                new { id = createdEvent.Id },
                createdEvent);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EventResponseDto>> GetEventById(int id)
    {
        try
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (email is null)
            {
                return BadRequest("User email not found in claims.");
            }

            var userEvents = await _eventService.GetUserEvents(email);
            var requestedEvent = userEvents.FirstOrDefault(e => e.Id == id);

            if (requestedEvent == null)
            {
                return NotFound(new { Message = "Event not found or you don't have access to it." });
            }

            return Ok(requestedEvent);
        }
        catch (Exception ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    [Authorize(Roles = "Teacher,Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<EventResponseDto>> UpdateEvent(int id, [FromBody] EventUpdateDto eventDto)
    {
        try
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            if (email is null)
            {
                return BadRequest("User email not found in claims.");
            }

            var updatedEvent = await _eventService.UpdateEventAsync(id, eventDto, email);
            return Ok(updatedEvent);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    [Authorize(Roles = "Teacher,Admin")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteEvent(int id)
    {
        try
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            if (email is null)
            {
                return BadRequest("User email not found in claims.");
            }

            var result = await _eventService.DeleteEventAsync(id, email);

            if (result)
            {
                return NoContent(); // 204 No Content - successful deletion
            }

            return BadRequest(new { Message = "Failed to delete event." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { ex.Message });
        }
    }
}