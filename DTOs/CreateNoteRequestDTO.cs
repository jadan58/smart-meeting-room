namespace SmartMeetingRoomAPI.DTOs
{
    public class CreateNoteRequestDto
    {
        public string Content { get; set; }
        public Guid CreatedByUserId { get; set; }
    }
}
