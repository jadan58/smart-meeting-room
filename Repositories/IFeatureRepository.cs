using SmartMeetingRoomAPI.Models;

namespace SmartMeetingRoomAPI.Repositories
{
    public interface IFeatureRepository
    {
        Task<List<Feature>> GetAllAsync();
        Task<Feature?> GetByIdAsync(Guid id);
        Task<List<Feature>> GetByIdsAsync(List<Guid> ids);
        Task<Feature> AddAsync(Feature feature);
        Task<Feature> UpdateAsync(Feature feature);
        Task<bool> DeleteAsync(Guid id);
    }
}
