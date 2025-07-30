using System.ComponentModel.DataAnnotations;

namespace SmartMeetingRoomAPI.DTOs.Auth
{
    public class RegisterRequestDto
    {
        [Required, MaxLength(50)]
        public string FirstName { get; set; }

        [Required, MaxLength(50)]
        public string LastName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
