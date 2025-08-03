using Microsoft.EntityFrameworkCore;
using SmartMeetingRoomAPI.Data;
using SmartMeetingRoomAPI.Models;

public class SQLNotificationsRepository : INotificationsRepository
{
    private readonly AppDbContext _context;

    public SQLNotificationsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Notification>> GetUserNotificationsAsync(Guid userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.Date)
            .ToListAsync();
    }

    public async Task<Notification?> GetByIdAsync(Guid id)
    {
        return await _context.Notifications.FindAsync(id);
    }

    public async Task CreateAsync(Notification notification)
    {
        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();
    }

    public async Task MarkAsReadAsync(Guid id)
    {
        var notif = await _context.Notifications.FindAsync(id);
        if (notif != null && !notif.IsRead)
        {
            notif.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var notif = await _context.Notifications.FindAsync(id);
        if (notif != null)
        {
            _context.Notifications.Remove(notif);
            await _context.SaveChangesAsync();
        }
    }
}
