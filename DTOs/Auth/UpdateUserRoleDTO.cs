using System.ComponentModel.DataAnnotations;

namespace SmartMeetingRoomAPI.DTOs.Auth
{
    public class UpdateUserRoleDTO
    {
        [Required]
        public string Role { get; set; } // "Guest" or "Employee" or "Admin"

    }
}
