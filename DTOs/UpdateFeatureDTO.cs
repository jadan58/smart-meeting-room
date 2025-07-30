using System.ComponentModel.DataAnnotations;

namespace SmartMeetingRoomAPI.DTOs
{
    public class UpdateFeatureDTO
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
    }
}
