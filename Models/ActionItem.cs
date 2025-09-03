using System;
using System.ComponentModel.DataAnnotations;

namespace SmartMeetingRoomAPI.Models
{
    public class ActionItem
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid MeetingId { get; set; }
        public Meeting Meeting { get; set; }

        [Required]
        public Guid AssignedToUserId { get; set; }
        [Required]
        public ApplicationUser AssignedToUser { get; set; }

        public List<string>?AssignmentAttachmentsUrl { get; set; }

        public List<string>? SubmissionAttachmentsUrl{ get; set; }
        public DateTime Deadline { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Pending";
        [MaxLength(20)]
        public string Judgment { get; set; } = "Unjudged";

        [MaxLength(20)]
        public string Type { get; set; } // Decision, Task, Issue

        public string Description { get; set; }
    }
}
