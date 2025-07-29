using System;

namespace SmartMeetingRoomAPI.DTOs
{
    public class AttachmentDto
    {
        public Guid Id { get; set; }
        public string Url { get; set; }
        public string FileName { get; set; }
        public DateTime UploadedAt { get; set; }
        public Guid UploadedByUserId { get; set; }
    }
}
