using System;
using System.ComponentModel.DataAnnotations;

namespace SmartMeetingRoomAPI.Models
{
    public class Notification
    {
        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(100)]
        public string Subject { get; set; }

        [Required]
        public string Body { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
    }
}
