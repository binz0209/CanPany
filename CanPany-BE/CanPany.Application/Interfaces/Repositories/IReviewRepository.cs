using CanPany.Domain.Entities;

namespace CanPany.Application.Interfaces.Repositories;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(string id);
    Task<IEnumerable<Review>> GetByProjectIdAsync(string projectId);
    Task<IEnumerable<Review>> GetByUserAsync(string userId); // cả reviewer lẫn reviewee
    Task<Review> InsertAsync(Review entity);
    Task<bool> DeleteAsync(string id);
}
