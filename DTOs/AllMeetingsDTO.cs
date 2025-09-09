using System;
using System.Collections.Generic;

namespace SmartMeetingRoomAPI.DTOs
{
    public class AllMeetingsDto
    {
        public Guid Id { get; set; }
        public Guid RoomId { get; set; }
        public Guid RecurringBookingId { get; set; }
        public Guid UserId { get; set; } // Organizer
        public string Title { get; set; }
        public string Agenda { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; }
        public string? onlineLink { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
