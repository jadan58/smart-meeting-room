using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartMeetingRoomAPI.Models
{
    public class Feature
    {
        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } // Projector, Whiteboard

        // Navigation
        public ICollection<RoomFeature> RoomFeatures { get; set; } = new List<RoomFeature>();
    }
}
