using SmartMeetingRoomAPI.DTOs;

public class RecurringBookingResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public int TotalMeetings { get; set; }
    public string Pattern { get; set; }
    public DateTime RecurrenceEndDate { get; set; }
}
