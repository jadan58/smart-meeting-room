using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartMeetingRoomAPI.Models
{
    public class RecurringBooking
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        [MaxLength(50)]
        public string RecurrencePattern { get; set; } // Daily, Weekly, etc.

        public DateTime RecurrenceEndDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Meeting> Meetings { get; set; } = new List<Meeting>();
    }
}
