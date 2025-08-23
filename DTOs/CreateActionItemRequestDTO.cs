using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;

namespace SmartMeetingRoomAPI.DTOs
{
    public class CreateActionItemRequestDto
    {
        public string Description { get; set; }
        public string Type { get; set; }
        public DateTime Deadline { get; set; }
        public Guid AssignedToUserId { get; set; }
    }
}
