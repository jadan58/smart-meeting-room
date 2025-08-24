namespace SmartMeetingRoomAPI.DTOs
{
    public class MeetingWithInviteDto
    {
        public Guid InviteId { get; set; }
        public AllMeetingsDto Meeting { get; set; }
    }
}
