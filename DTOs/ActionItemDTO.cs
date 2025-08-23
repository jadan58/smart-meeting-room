using SmartMeetingRoomAPI.Models;
using System;

namespace SmartMeetingRoomAPI.DTOs
{
    public class ActionItemDto
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string Judgment { get; set; }
        public DateTime Deadline { get; set; }
        public Guid AssignedToUserId { get; set; } // for reference

    }
}
