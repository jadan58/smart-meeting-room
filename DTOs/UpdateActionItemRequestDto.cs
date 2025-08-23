using System;
using System.ComponentModel.DataAnnotations;

namespace SmartMeetingRoomAPI.DTOs
{
    public class UpdateActionItemRequestDto
    {
        [Required]
        public Guid AssignedToUserId { get; set; }

        public DateTime? Deadline { get; set; }

        [MaxLength(20)]
        public string Status { get; set; }
        [MaxLength(20)]
        public string Judgement { get; set; }

        [MaxLength(20)]
        public string Type { get; set; } // e.g., Decision, Task, Issue

        public string Description { get; set; }
    }
}
