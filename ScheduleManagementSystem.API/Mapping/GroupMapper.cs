using ScheduleManagementSystem.API.Models;
using ScheduleManagementSystem.Shared.DTOs;

namespace ScheduleManagementSystem.API.Mapping;

public static class GroupMapper
{
    public static Group FromGroupResponseDto(GroupResponseDto groupResponseDto)
    {
        return new Group
        {
            Id = groupResponseDto.Id,
            Name = groupResponseDto.Name,
            Description = groupResponseDto.Description,
            Users = groupResponseDto.Users.Select(UserMapper.FromUserSummaryDto).ToList(),
            Events = groupResponseDto.Events.Select(EventMapper.FromEventSummaryDto).ToList()
        };
    }

    public static Group FromGroupSummaryDto(GroupSummaryDto groupSummaryDto)
    {
        return new Group
        {
            Id = groupSummaryDto.Id,
            Name = groupSummaryDto.Name,
            Description = groupSummaryDto.Description
        };
    }

    public static GroupResponseDto ToGroupResponseDto(Group group)
    {
        return new GroupResponseDto
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            Users = group.Users.Select(UserMapper.ToUserSummaryDto).ToList(),
            Events = group.Events.Select(EventMapper.ToEventSummaryDto).ToList()
        };
    }

    public static GroupSummaryDto ToGroupSummaryDto(Group group)
    {
        return new GroupSummaryDto
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description
        };
    }
}
