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


        public bool Attended { get; set; } = false;

        [MaxLength(20)]
        public string Status { get; set; } // Invited, Declined
    }
}
