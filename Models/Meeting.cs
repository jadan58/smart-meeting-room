using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace SmartMeetingRoomAPI.Models
{
    public class Meeting
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid? RoomId { get; set; }
        public Room? Room { get; set; }

        public Guid? UserId { get; set; } // Organizer
        public ApplicationUser? User { get; set; }

        public Guid? RecurringBookingId { get; set; }
        public RecurringBooking RecurringBooking { get; set; }
        public string? onlineLink{ get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } // Scheduled, Completed

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Guid? NextMeetingId { get; set; }
        public Meeting NextMeeting { get; set; }

        [MaxLength(200)]
        public string Title { get; set; }

        public string Agenda { get; set; }

        // Navigation
        public ICollection<Invitee> Invitees { get; set; } = new List<Invitee>();
        public ICollection<Note> Notes { get; set; } = new List<Note>();
        public ICollection<ActionItem> ActionItems { get; set; } = new List<ActionItem>();
        public List<string>? AttachmentUrls { get; set; }
    }
}
