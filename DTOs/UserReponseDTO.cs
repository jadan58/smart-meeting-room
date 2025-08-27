using Microsoft.AspNetCore.Identity;
using SmartMeetingRoomAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace SmartMeetingRoomAPI.DTOs
{
    public class UserReponseDTO
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
        public string Email { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<string> Roles { get; set; } 

       
    }
}
