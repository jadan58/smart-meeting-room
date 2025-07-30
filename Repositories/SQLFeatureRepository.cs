using Microsoft.EntityFrameworkCore;
using SmartMeetingRoomAPI.Data;
using SmartMeetingRoomAPI.Models;

namespace SmartMeetingRoomAPI.Repositories
{
    public class SqlFeatureRepository : IFeatureRepository
    {
        private readonly AppDbContext _dbContext;

        public SqlFeatureRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Feature>> GetAllAsync()
        {
            return await _dbContext.Features.ToListAsync();
        }

        public async Task<Feature?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Features.FindAsync(id);
        }

        public async Task<List<Feature>> GetByIdsAsync(List<Guid> ids)
        {
            return await _dbContext.Features
                .Where(f => ids.Contains(f.Id))
                .ToListAsync();
        }

        public async Task<Feature> AddAsync(Feature feature)
        {
            await _dbContext.Features.AddAsync(feature);
            await _dbContext.SaveChangesAsync();
            return feature;
        }

        public async Task<Feature> UpdateAsync(Feature feature)
        {
            _dbContext.Features.Update(feature);
            await _dbContext.SaveChangesAsync();
            return feature;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var feature = await _dbContext.Features.FindAsync(id);
            if (feature == null)
                return false;

            _dbContext.Features.Remove(feature);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
