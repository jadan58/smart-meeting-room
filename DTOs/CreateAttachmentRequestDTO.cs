namespace SmartMeetingRoomAPI.DTOs
{
    public class CreateAttachmentRequestDto
    {
        public string Url { get; set; }
        public string FileName { get; set; }
        public Guid UploadedByUserId { get; set; }
    }
}
