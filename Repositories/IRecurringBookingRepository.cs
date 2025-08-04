using SmartMeetingRoomAPI.Models;

namespace SmartMeetingRoomAPI.Repositories
{
    public interface IRecurringBookingRepository
    {
        Task <RecurringBooking> createAsync(RecurringBooking recurringBooking);
    }
}
