using Microsoft.EntityFrameworkCore;
using ScheduleManagementSystem.API.Data;
using ScheduleManagementSystem.API.Models;
using ScheduleManagementSystem.API.Mapping;
using ScheduleManagementSystem.Shared.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScheduleManagementSystem.API.Services;

public class GroupService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<List<GroupSummaryDto>> GetAllGroupsAsync()
    {
        var groups = await _context.Groups.ToListAsync();
        return groups.Select(GroupMapper.ToGroupSummaryDto).ToList();
    }

    public async Task<GroupResponseDto> GetGroupByIdAsync(int id)
    {
        var group = await _context.Groups
            .Include(g => g.Users)
            .Include(g => g.Events)
            .FirstOrDefaultAsync(g => g.Id == id);

        return group == null 
            ? throw new KeyNotFoundException($"Group with ID {id} not found.") 
            : GroupMapper.ToGroupResponseDto(group);
    }

    public async Task<List<GroupSummaryDto>> GetGroupsByNameAsync(string name)
    {
        var groups = await _context.Groups
            .Where(g => g.Name.Contains(name))
            .ToListAsync();

        return groups.Select(GroupMapper.ToGroupSummaryDto).ToList();
    }

    public async Task<List<GroupSummaryDto>> GetUserGroupsAsync(string userEmail)
    {
        var user = await _context.Users
            .Include(u => u.Groups)
            .FirstOrDefaultAsync(u => u.Email == userEmail);

        return user == null
            ? throw new KeyNotFoundException($"User with email {userEmail} not found.")
            : user.Groups.Select(GroupMapper.ToGroupSummaryDto).ToList();
    }

    public async Task<GroupResponseDto> CreateGroupAsync(string name, string description)
    {
        var group = new Group
        {
            Name = name,
            Description = description
        };

        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        return GroupMapper.ToGroupResponseDto(group);
    }

    public async Task<GroupResponseDto> UpdateGroupAsync(int id, string? name, string? description)
    {
        var group = await _context.Groups.FindAsync(id) 
            ?? throw new KeyNotFoundException($"Group with ID {id} not found.");
        
        group.Name = name ?? group.Name;
        group.Description = description ?? group.Description;

        _context.Groups.Update(group);
        await _context.SaveChangesAsync();

        return GroupMapper.ToGroupResponseDto(group);
    }

    public async Task AddUserToGroupAsync(int groupId, int userId)
    {
        var group = await _context.Groups
            .Include(g => g.Users)
            .FirstOrDefaultAsync(g => g.Id == groupId) 
            ?? throw new KeyNotFoundException($"Group with ID {groupId} not found.");
        
        var user = await _context.Users.FindAsync(userId) 
            ?? throw new KeyNotFoundException($"User with ID {userId} not found.");

        if (group.Users.Any(u => u.Id == userId))
            return; // User already in group

        group.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveUserFromGroupAsync(int groupId, int userId)
    {
        var group = await _context.Groups
            .Include(g => g.Users)
            .FirstOrDefaultAsync(g => g.Id == groupId) ?? throw new KeyNotFoundException($"Group with ID {groupId} not found.");
        
        var user = group.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
            return; // User not in group

        group.Users.Remove(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteGroupAsync(int id)
    {
        var group = await _context.Groups.FindAsync(id)
            ?? throw new KeyNotFoundException($"Group with ID {id} not found.");
        
        _context.Groups.Remove(group);
        await _context.SaveChangesAsync();
    }
}