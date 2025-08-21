using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace SmartMeetingRoomAPI.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        [Required, MaxLength(50)]
        public string FirstName { get; set; }

        [Required, MaxLength(50)]
        public string LastName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int TotalMeetingTime { get; set; } = 0;
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        public ICollection<Meeting> OrganizedMeetings { get; set; } = new List<Meeting>();
        public ICollection<Meeting> InvitedMeetings { get; set; } = new List<Meeting>();
    }
}
