using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartMeetingRoomAPI.Models
{
    public class Room
    {
        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        public int Capacity { get; set; }

        [MaxLength(200)]
        public string Location { get; set; }

        [MaxLength(300)]
        public string? ImageUrl { get; set; }


        // Navigation
        public ICollection<Meeting> Meetings { get; set; } = new List<Meeting>();
        public ICollection<RoomFeature> RoomFeatures { get; set; } = new List<RoomFeature>();
    }
}
