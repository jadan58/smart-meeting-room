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

        public async Task<Room?> UpdateAsync(Guid id, Room updatedRoom)
        {
            var existingRoom = await dbContext.Rooms.FindAsync(id);

            if (existingRoom == null)
                return null;

            existingRoom.Name = updatedRoom.Name;
            existingRoom.Capacity = updatedRoom.Capacity;
            existingRoom.Location = updatedRoom.Location;
            

            await dbContext.SaveChangesAsync();
            return existingRoom;
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
