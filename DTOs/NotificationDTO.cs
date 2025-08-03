public class NotificationDto
{
    public Guid Id { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public DateTime Date { get; set; }
    public bool IsRead { get; set; }
}
