using Microsoft.AspNetCore.Mvc;
using ScheduleManagementSystem.Shared.DTOs;
using ScheduleManagementSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace ScheduleManagementSystem.API.Controllers;

//[Authorize]
[ApiController]
[Route("api/[controller]")]
public class GroupController(GroupService groupService) : ControllerBase
{
    private readonly GroupService _groupService = groupService;

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<GroupSummaryDto>>> GetUserGroups()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (email is null)
        {
            return BadRequest("User email not found in claims.");
        }

        try
        {
            var groups = await _groupService.GetUserGroupsAsync(email);
            return Ok(groups);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    [Authorize(Roles = "Teacher,Admin")]
    [HttpGet("all")]
    public async Task<ActionResult<List<GroupSummaryDto>>> GetAllGroups()
    {
        try
        {
            var groups = await _groupService.GetAllGroupsAsync();
            return Ok(groups);
        }
        catch (Exception ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<GroupResponseDto>> GetGroupById(int id)
    {
        try
        {
            var group = await _groupService.GetGroupByIdAsync(id);
            return Ok(group);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    [Authorize]
    [HttpGet("search")]
    public async Task<ActionResult<List<GroupSummaryDto>>> SearchGroups([FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("Search name cannot be empty.");
        }

        try
        {
            var groups = await _groupService.GetGroupsByNameAsync(name);
            return Ok(groups);
        }
        catch (Exception ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    [Authorize(Roles = "Teacher,Admin")]
    [HttpPost]
    public async Task<ActionResult<GroupResponseDto>> CreateGroup([FromBody] GroupCreateDto groupDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var createdGroup = await _groupService.CreateGroupAsync(groupDto.Name, groupDto.Description);
            return CreatedAtAction(
                nameof(GetGroupById),
                new { id = createdGroup.Id },
                createdGroup);
        }
        catch (Exception ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    [Authorize(Roles = "Teacher,Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<GroupResponseDto>> UpdateGroup(int id, [FromBody] GroupUpdateDto groupDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updatedGroup = await _groupService.UpdateGroupAsync(id, groupDto.Name, groupDto.Description);
            return Ok(updatedGroup);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    [Authorize(Roles = "Teacher,Admin")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteGroup(int id)
    {
        try
        {
            await _groupService.DeleteGroupAsync(id);
            return NoContent(); // 204 No Content
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    [Authorize(Roles = "Teacher,Admin")]
    [HttpPost("{id:int}/users/{userId:int}")]
    public async Task<ActionResult> AddUserToGroup(int id, int userId)
    {
        try
        {
            await _groupService.AddUserToGroupAsync(id, userId);
            return Ok(new { Message = "User added to group successfully." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    [Authorize(Roles = "Teacher,Admin")]
    [HttpDelete("{id:int}/users/{userId:int}")]
    public async Task<ActionResult> RemoveUserFromGroup(int id, int userId)
    {
        try
        {
            await _groupService.RemoveUserFromGroupAsync(id, userId);
            return Ok(new { Message = "User removed from group successfully." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    [Authorize]
    [HttpPost("{id:int}/join")]
    public async Task<ActionResult> JoinGroup(int id)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdString is null || !int.TryParse(userIdString, out int userId))
        {
            return BadRequest("User ID not found in claims.");
        }

        try
        {
            await _groupService.AddUserToGroupAsync(id, userId);
            return Ok(new { Message = "Successfully joined the group." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    [Authorize]
    [HttpPost("{id:int}/leave")]
    public async Task<ActionResult> LeaveGroup(int id)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdString is null || !int.TryParse(userIdString, out int userId))
        {
            return BadRequest("User ID not found in claims.");
        }

        try
        {
            await _groupService.RemoveUserFromGroupAsync(id, userId);
            return Ok(new { Message = "Successfully left the group." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { ex.Message });
        }
    }
}