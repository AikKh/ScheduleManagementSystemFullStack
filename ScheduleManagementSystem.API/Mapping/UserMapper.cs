using ScheduleManagementSystem.Shared.DTOs;
using ScheduleManagementSystem.API.Models;
using ScheduleManagementSystem.Shared.Enums;

namespace ScheduleManagementSystem.API.Mapping;

public static class UserMapper
{
    public static User FromUserResponseDto(UserResponseDto userResponseDto)
    {
        return new User
        {
            Id = userResponseDto.Id,
            Username = userResponseDto.Username,
            Email = userResponseDto.Email,
            Role = Enum.Parse<UserRole>(userResponseDto.Role),
            Groups = userResponseDto.Groups.Select(GroupMapper.FromGroupSummaryDto).ToList(),
            AuthMethods = userResponseDto.AuthMethods.Select(AuthMethodMapper.FromAuthMethodSummaryDto).ToList()
        };
    }

    public static User FromUserSummaryDto(UserSummaryDto userSummaryDto)
    {
        return new User
        {
            Id = userSummaryDto.Id,
            Username = userSummaryDto.Username,
            Email = userSummaryDto.Email,
            Role = Enum.Parse<UserRole>(userSummaryDto.Role)
        };
    }

    public static UserResponseDto ToUserResponseDto(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.ToString(),
            Groups = user.Groups.Select(GroupMapper.ToGroupSummaryDto).ToList(),
            AuthMethods = user.AuthMethods.Select(AuthMethodMapper.ToAuthMethodSummaryDto).ToList()
        };
    }

    public static UserSummaryDto ToUserSummaryDto(User user)
    {
        return new UserSummaryDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }
}
