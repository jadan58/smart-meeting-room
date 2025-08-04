namespace SmartMeetingRoomAPI.DTOs
{
    public class CreateRecurringMeetingRequestDTO
    {
        public Guid RoomId { get; set; }
        public string Title { get; set; }
        public string Agenda { get; set; }

        public string RecurrencePattern { get; set; } // Daily, Weekly, Monthly
        public DateTime RecurrenceEndDate { get; set; } // Until when

        public DateTime StartTime { get; set; } // First meeting start
        public DateTime EndTime { get; set; }   // First meeting end
    }
}
