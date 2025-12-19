using CanPany.Application.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;

namespace CanPany.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repo;

    public CategoryService(ICategoryRepository repo)
    {
        _repo = repo;
    }

    public Task<IEnumerable<Category>> GetAllAsync()
        => _repo.GetAllAsync();

    public Task<Category?> GetByIdAsync(string id)
        => _repo.GetByIdAsync(id);

    public Task<Category> CreateAsync(Category entity)
        => _repo.InsertAsync(entity);

    public async Task<bool> UpdateAsync(string id, Category entity)
    {
        entity.Id = id;
        return await _repo.UpdateAsync(entity);
    }

    public Task<bool> DeleteAsync(string id)
        => _repo.DeleteAsync(id);
}
