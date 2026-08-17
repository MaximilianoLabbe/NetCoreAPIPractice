using API.Data.Entities;
using API.Data.Models;
using API.Data.Queries;

namespace API.Data.Repositories.Interfaces;

public interface IProductRepository
{
    Task<PagedDataResult<Product>> GetAllAsync(
        ProductQueryOptions query,
        CancellationToken cancellationToken = default);

    Task<Product?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Product> CreateAsync(
        Product product,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Product product,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Product product,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
    string name,
    int? excludeId = null,
    CancellationToken cancellationToken = default);
}