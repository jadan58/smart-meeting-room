using SmartMeetingRoomAPI.Data;
using SmartMeetingRoomAPI.Models;

namespace SmartMeetingRoomAPI.Repositories
{
    public class SQLRecurringRepository : IRecurringBookingRepository
    {
        private readonly AppDbContext dbContext;

        public SQLRecurringRepository(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<RecurringBooking> createAsync(RecurringBooking recurringBooking)
        {
            await dbContext.RecurringBookings.AddAsync(recurringBooking);
            await dbContext.SaveChangesAsync();
            return recurringBooking;
        }
    }
}
