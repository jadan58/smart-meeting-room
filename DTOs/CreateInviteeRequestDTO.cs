namespace SmartMeetingRoomAPI.DTOs
{
    public class CreateInviteeRequestDto
    {
        public Guid UserId { get; set; }
        public string Status { get; set; }
    }
}
