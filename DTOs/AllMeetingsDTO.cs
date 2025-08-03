using System;
using System.Collections.Generic;

namespace SmartMeetingRoomAPI.DTOs
{
    public class AllMeetingsDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Agenda { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
