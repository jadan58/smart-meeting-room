namespace SmartMeetingRoomAPI.DTOs
{
    public class ApplicationUserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public List<string> Roles { get; set; } 
    }
}
