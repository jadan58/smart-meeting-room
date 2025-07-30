using System;

namespace SmartMeetingRoomAPI.DTOs
{
    public class CreateMeetingRequestDto
    {
        public string Title { get; set; }
        public string Agenda { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; }

        public Guid RoomId { get; set; }
        public Guid UserId { get; set; }

        // Recurrence optional
        public string RecurrencePattern { get; set; }
        public DateTime? RecurrenceEndDate { get; set; }
    }
}
