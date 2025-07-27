using System;
using System.ComponentModel.DataAnnotations;

namespace SmartMeetingRoomAPI.Models
{
    public class RoomFeature
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid RoomId { get; set; }
        public Room Room { get; set; }

        [Required]
        public Guid FeatureId { get; set; }
        public Feature Feature { get; set; }
    }
}
