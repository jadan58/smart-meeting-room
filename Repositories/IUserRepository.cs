using SmartMeetingRoomAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartMeetingRoomAPI.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<ApplicationUser>> GetAllAsync();
        Task<ApplicationUser?> GetByIdAsync(Guid id);
        Task<ApplicationUser?> GetByEmailAsync(string email);
    }
}
