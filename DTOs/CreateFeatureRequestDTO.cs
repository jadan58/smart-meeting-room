using System.ComponentModel.DataAnnotations;

namespace SmartMeetingRoomAPI.DTOs
{
    public class CreateFeatureRequestDTO
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
    }
}
