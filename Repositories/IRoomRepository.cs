using SmartMeetingRoomAPI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartMeetingRoomAPI.Repositories
{
    public interface IRoomRepository
    {
        Task<IEnumerable<Room>> GetAllAsync();
        Task<Room?> GetByIdAsync(Guid id);
        Task<Room> AddAsync(Room room);
        Task<Room?> UpdateAsync(Guid id, Room room);
        Task<Room?> DeleteAsync(Guid id);
    }
}
