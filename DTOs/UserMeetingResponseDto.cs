namespace SmartMeetingRoomAPI.DTOs
{
    public class UserMeetingResponseDto
    {
        public ICollection<AllMeetingsDto> OrganizedMeetings { get; set; } = new List<AllMeetingsDto>();
        public ICollection<AllMeetingsDto> InvitedMeetings { get; set; } = new List<AllMeetingsDto>();
    }
}
