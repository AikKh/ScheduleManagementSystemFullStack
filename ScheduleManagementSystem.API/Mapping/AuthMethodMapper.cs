using ScheduleManagementSystem.API.Models;
using ScheduleManagementSystem.Shared.DTOs;

namespace ScheduleManagementSystem.API.Mapping;

public static class AuthMethodMapper
{
    public static AuthMethod FromAuthMethodSummaryDto(AuthMethodSummaryDto authMethodDto)
    {
        return new AuthMethod
        {
            Id = authMethodDto.Id,
            UserId = authMethodDto.UserId,
            Provider = authMethodDto.Provider
        };
    }

    public static AuthMethod FromAuthMethodResponseDto(AuthMethodResponseDto authMethodDto)
    {
        return new AuthMethod
        {
            Id = authMethodDto.Id,
            UserId = authMethodDto.UserId,
            Provider = authMethodDto.Provider,
            ProviderUserId = authMethodDto.ProviderUserId,
            PasswordHash = authMethodDto.PasswordHash,
        };
    }

    public static AuthMethodSummaryDto ToAuthMethodSummaryDto(AuthMethod authMethod)
    {
        return new AuthMethodSummaryDto
        {
            Id = authMethod.Id,
            UserId = authMethod.UserId,
            Provider = authMethod.Provider
        };
    }

    public static AuthMethodResponseDto ToAuthMethodResponseDto(AuthMethod authMethod)
    {
        return new AuthMethodResponseDto
        {
            Id = authMethod.Id,
            UserId = authMethod.UserId,
            Provider = authMethod.Provider,
            ProviderUserId = authMethod.ProviderUserId,
            PasswordHash = authMethod.PasswordHash,
            User = authMethod.User != null ? UserMapper.ToUserSummaryDto(authMethod.User) : null
        };
    }

    public static void UpdateAuthMethodFromDto(AuthMethod authMethod, AuthMethodUpdateDto updateDto)
    {
        if (!string.IsNullOrEmpty(updateDto.PasswordHash))
        {
            authMethod.PasswordHash = updateDto.PasswordHash;
        }
    }
}