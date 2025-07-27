using System;
using System.ComponentModel.DataAnnotations;

namespace SmartMeetingRoomAPI.Models
{
    public class Note
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid MeetingId { get; set; }
        public Meeting Meeting { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public Guid CreatedByUserId { get; set; }
        public ApplicationUser CreatedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
