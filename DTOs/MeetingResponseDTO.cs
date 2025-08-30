using System;
using System.Collections.Generic;

namespace SmartMeetingRoomAPI.DTOs
{
    public class MeetingResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Agenda { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Guid RoomId { get; set; }
        public Guid UserId { get; set; }
        public Guid? RecurringBookingId { get; set; }
        public Guid? NextMeetingId { get; set; }

        public List<NoteDto> Notes { get; set; }
        public List<ActionItemDto> ActionItems { get; set; }
        public List<InviteeDto> Invitees { get; set; }
        public List<string>? AttachmentUrls { get; set; }
    }
}
