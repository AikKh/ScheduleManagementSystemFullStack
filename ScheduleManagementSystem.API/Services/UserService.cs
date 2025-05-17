using Microsoft.EntityFrameworkCore;
using ScheduleManagementSystem.API.Data;
using ScheduleManagementSystem.Shared.DTOs;
using ScheduleManagementSystem.API.Mapping;
using ScheduleManagementSystem.API.Models;

namespace ScheduleManagementSystem.API.Services;

public class UserService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<bool> UserExistsAsync(string email)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        return user != null;
    }
    public async Task<List<UserSummaryDto>> GetAllUsersAsync()
    {
        var users = await _context.Users
            .ToListAsync();

        return users.Select(UserMapper.ToUserSummaryDto).ToList();
    }

    public async Task<UserResponseDto> GetUserByIdAsync(int id)
    {
        var user = await _context.Users
            //.Include(u => u.Groups)
            //.ThenInclude(g => g.Users)
            .Include(u => u.AuthMethods)
            .FirstOrDefaultAsync(u => u.Id == id);

        return user == null
            ? throw new Exception($"User with ID {id} not found.")
            : UserMapper.ToUserResponseDto(user);
    }

    public async Task<UserResponseDto> GetUserByEmailAsync(string email)
    {
        var user =  await _context.Users
            .Include(u => u.Groups)
            .Include(u => u.AuthMethods)
            .FirstOrDefaultAsync(u => u.Email == email);

        return user == null
            ? throw new Exception($"User with email <{email}> not found.")
            : UserMapper.ToUserResponseDto(user);
    }

    public async Task<User> CreateUserAsync(User user, AuthMethod authMethod)
    {
        if (await UserExistsAsync(user.Email))
            throw new InvalidOperationException("User already exists");

        user.AuthMethods = [authMethod];
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }
}
