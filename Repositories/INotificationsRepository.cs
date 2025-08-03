using SmartMeetingRoomAPI.Models;

public interface INotificationsRepository
{
    Task<List<Notification>> GetUserNotificationsAsync(Guid userId);
    Task<Notification?> GetByIdAsync(Guid id);
    Task CreateAsync(Notification notification);
    Task MarkAsReadAsync(Guid id);
    Task DeleteAsync(Guid id);
}
