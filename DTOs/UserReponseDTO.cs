using Microsoft.AspNetCore.Identity;
using SmartMeetingRoomAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace SmartMeetingRoomAPI.DTOs
{
    public class UserReponseDTO
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<string> Roles { get; set; } 

        public ICollection<AllMeetingsDto> OrganizedMeetings { get; set; } = new List<AllMeetingsDto>();
        public ICollection<InviteeDto> InvitedMeetings { get; set; } = new List<InviteeDto>();
    }
}
