using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartMeetingRoomAPI.Models
{
    public class Role
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Title { get; set; } // Admin, Organizer, Member...

        // Navigation
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}
