using System;
using System.ComponentModel.DataAnnotations;

namespace SmartMeetingRoomAPI.Models
{
    public class Invitee
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid MeetingId { get; set; }
        public Meeting Meeting { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }

        public string Email { get; set; }

        public string Status { get; set; }="Pending"; // Pending, Answered

        public string Attendance { get; set; } = "Declined"; // Accpeted, Declined
    }
}
