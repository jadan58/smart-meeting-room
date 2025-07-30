using Microsoft.EntityFrameworkCore;
using SmartMeetingRoomAPI.Data;
using SmartMeetingRoomAPI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartMeetingRoomAPI.Repositories
{
    public class SqlRoomRepository : IRoomRepository
    {
        private readonly AppDbContext dbContext;

        public SqlRoomRepository(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IEnumerable<Room>> GetAllAsync()
        {
            return await dbContext.Rooms
                .Include(r => r.RoomFeatures)
                    .ThenInclude(rf => rf.Feature)
                .ToListAsync();
        }

        public async Task<Room?> GetByIdAsync(Guid id)
        {
            return await dbContext.Rooms
                .Include(r => r.RoomFeatures)
                    .ThenInclude(rf => rf.Feature)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Room> AddAsync(Room room)
        {
            await dbContext.Rooms.AddAsync(room);
            await dbContext.SaveChangesAsync();
            return room;
        }

        public async Task<Room> UpdateAsync(Guid id, Room room)
        {
            dbContext.Attach(room);
            dbContext.Entry(room).State = EntityState.Modified;

            // Also mark child entities (RoomFeatures) as added
            foreach (var rf in room.RoomFeatures)
            {
                dbContext.Entry(rf).State = EntityState.Added;
            }

            await dbContext.SaveChangesAsync();
            return room;
        }


        public async Task<Room?> DeleteAsync(Guid id)
        {
            var room = await dbContext.Rooms.FindAsync(id);
            if (room == null)
                return null;

            dbContext.Rooms.Remove(room);
            await dbContext.SaveChangesAsync();
            return room;
        }
    }
}
