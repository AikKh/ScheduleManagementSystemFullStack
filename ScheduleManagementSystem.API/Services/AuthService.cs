using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using ScheduleManagementSystem.API.Models;
using ScheduleManagementSystem.API.Data;
using ScheduleManagementSystem.API.Mapping;
using ScheduleManagementSystem.Shared.DTOs;
using ScheduleManagementSystem.Shared.Enums;

namespace ScheduleManagementSystem.API.Services;

public class AuthService(AppDbContext context, JwtService jwtService)
{
    private readonly AppDbContext _context = context;
    private readonly JwtService _jwtService = jwtService;

    public async Task<(UserResponseDto, string)> AuthenticateLocalAsync(string email, string password)
    {
        var user = await _context.Users
            .Include(u => u.AuthMethods)
            .FirstOrDefaultAsync(u => u.Email == email) 
            ?? throw new InvalidOperationException("User not found");

        var authMethod = user.AuthMethods.FirstOrDefault(a => a.Provider == AuthProvider.Local)
            ?? throw new InvalidOperationException("User does not have local authentication");

        if (!BCrypt.Net.BCrypt.Verify(password, authMethod.PasswordHash))
            throw new InvalidOperationException("Invalid password");

        var token = _jwtService.GenerateJwtToken(user);
        var user_dto = UserMapper.ToUserResponseDto(user);
        return (user_dto, token);
    }
}