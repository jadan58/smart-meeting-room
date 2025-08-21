using Microsoft.EntityFrameworkCore;
using SmartMeetingRoomAPI.Data;
using SmartMeetingRoomAPI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartMeetingRoomAPI.Repositories
{
    public class SQLUserRepository : IUserRepository
    {
        private readonly AppDbContext dbContext;

        public SQLUserRepository(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllAsync()
        {
            return await dbContext.Users.ToListAsync();
        }

        public async Task<ApplicationUser?> GetByIdAsync(Guid id)
        {
            var user = await dbContext.Users
                .Include(u => u.OrganizedMeetings)
                .Include(u => u.InvitedMeetings)
                .FirstOrDefaultAsync(u => u.Id == id);
            return user;

        }

        public async Task<ApplicationUser?> GetByEmailAsync(string email)
        {
            return await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}
