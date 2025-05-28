using Microsoft.EntityFrameworkCore;
using ScheduleManagementSystem.API.Data;
using ScheduleManagementSystem.Shared.DTOs;
using ScheduleManagementSystem.API.Mapping;
using ScheduleManagementSystem.API.Models;
using ScheduleManagementSystem.Shared.Enums;

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
            .Include(u => u.Groups)
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

    /// <summary>
    /// Updates user profile information (username and/or email)
    /// </summary>
    public async Task<UserResponseDto> UpdateUserAsync(string currentEmail, UpdateUserDto updateUserDto)
    {
        var user = await _context.Users
            .Include(u => u.Groups)
            .Include(u => u.AuthMethods)
            .FirstOrDefaultAsync(u => u.Email == currentEmail)
            ?? throw new Exception($"User with email <{currentEmail}> not found.");

        if (!string.IsNullOrWhiteSpace(updateUserDto.Email) &&
            updateUserDto.Email != currentEmail &&
            await UserExistsAsync(updateUserDto.Email))
        {
            throw new InvalidOperationException("Email is already taken by another user.");
        }

        if (!string.IsNullOrWhiteSpace(updateUserDto.Username))
        {
            user.Username = updateUserDto.Username;
        }

        if (!string.IsNullOrWhiteSpace(updateUserDto.Email))
        {
            user.Email = updateUserDto.Email;
        }

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return UserMapper.ToUserResponseDto(user);
    }

    /// <summary>
    /// Changes user password (only for users with local authentication)
    /// </summary>
    public async Task ChangePasswordAsync(string email, string currentPassword, string newPassword)
    {
        var user = await _context.Users
            .Include(u => u.AuthMethods)
            .FirstOrDefaultAsync(u => u.Email == email)
            ?? throw new Exception($"User with email <{email}> not found.");

        // Find the local authentication method
        var localAuthMethod = user.AuthMethods.FirstOrDefault(a => a.Provider == AuthProvider.Local)
            ?? throw new InvalidOperationException("User does not have local authentication. Password cannot be changed.");

        // Verify the current password
        if (!BCrypt.Net.BCrypt.Verify(currentPassword, localAuthMethod.PasswordHash))
        {
            throw new InvalidOperationException("Current password is incorrect.");
        }

        // Hash and update the new password
        localAuthMethod.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

        _context.AuthMethods.Update(localAuthMethod);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes a user account permanently along with all related data
    /// </summary>
    public async Task DeleteUserAsync(string email, string password)
    {
        var user = await _context.Users
            .Include(u => u.AuthMethods)
            .Include(u => u.Groups)
            .FirstOrDefaultAsync(u => u.Email == email)
            ?? throw new Exception($"User with email <{email}> not found.");

        // For local auth users, verify password before deletion
        var localAuthMethod = user.AuthMethods.FirstOrDefault(a => a.Provider == AuthProvider.Local);
        if (localAuthMethod != null)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException("Password is required to delete account.");
            }

            if (!BCrypt.Net.BCrypt.Verify(password, localAuthMethod.PasswordHash))
            {
                throw new InvalidOperationException("Incorrect password. Account deletion failed.");
            }
        }
        // For Google auth users, we might want to skip password verification
        // or implement a different verification method

        // Remove the user (this will cascade delete auth methods due to EF configuration)
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }
}
