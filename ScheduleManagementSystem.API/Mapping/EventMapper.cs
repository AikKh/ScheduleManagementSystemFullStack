using ScheduleManagementSystem.API.Models;
using ScheduleManagementSystem.Shared.DTOs;

namespace ScheduleManagementSystem.API.Mapping
{
    public static class EventMapper
    {
        public static Event FromEventCreateDto(EventCreateDto eventDto)
        {
            return new Event
            {
                Title = eventDto.Title,
                Description = eventDto.Description,
                Date = eventDto.Date,
                StartTime = eventDto.StartTime,
                EndTime = eventDto.EndTime,
                Type = eventDto.Type,
                GroupId = eventDto.GroupId
            };
        }

        public static Event FromEventUpdateDto(EventUpdateDto eventDto)
        {
            return new Event
            {
                Title = eventDto.Title,
                Description = eventDto.Description,
                Date = eventDto.Date,
                StartTime = eventDto.StartTime,
                EndTime = eventDto.EndTime,
                Type = eventDto.Type,
                GroupId = eventDto.GroupId
            };
        }

        public static Event FromEventSummaryDto(EventSummaryDto eventDto)
        {
            return new Event
            {
                Id = eventDto.Id,
                Title = eventDto.Title,
                Description = eventDto.Description,
                Date = eventDto.Date,
                StartTime = eventDto.StartTime,
                EndTime = eventDto.EndTime,
                Type = eventDto.Type,
                GroupId = eventDto.GroupId
            };
        }

        public static void UpdateEventFromDto(Event eventEntity, EventUpdateDto eventDto)
        {
            eventEntity.Title = eventDto.Title;
            eventEntity.Description = eventDto.Description;
            eventEntity.Date = eventDto.Date;
            eventEntity.StartTime = eventDto.StartTime;
            eventEntity.EndTime = eventDto.EndTime;
            eventEntity.Type = eventDto.Type;
            eventEntity.GroupId = eventDto.GroupId;
        }

        public static EventSummaryDto ToEventSummaryDto(Event eventEntity)
        {
            return new EventSummaryDto
            {
                Id = eventEntity.Id,
                Title = eventEntity.Title,
                Description = eventEntity.Description,
                Date = eventEntity.Date,
                StartTime = eventEntity.StartTime,
                EndTime = eventEntity.EndTime,
                Type = eventEntity.Type,
                GroupId = eventEntity.GroupId
            };
        }

        public static EventResponseDto ToEventResponseDto(Event eventEntity)
        {
            return new EventResponseDto
            {
                Id = eventEntity.Id,
                Title = eventEntity.Title,
                Description = eventEntity.Description,
                Date = eventEntity.Date,
                StartTime = eventEntity.StartTime,
                EndTime = eventEntity.EndTime,
                Type = eventEntity.Type,
                GroupId = eventEntity.GroupId,
                Group = eventEntity.Group != null
                    ? GroupMapper.ToGroupSummaryDto(eventEntity.Group)
                    : null
            };
        }
    }
}