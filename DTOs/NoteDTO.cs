using System;

namespace SmartMeetingRoomAPI.DTOs
{
    public class NoteDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
