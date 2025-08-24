using System;

namespace SmartMeetingRoomAPI.DTOs
{
    public class InviteeDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public string Attendance { get; set; }
    }
}
