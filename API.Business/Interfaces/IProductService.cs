using API.Business.DTOs;

namespace API.Business.Interfaces;

public interface IProductService
{
    Task<PagedResultDto<ProductDto>> GetAllAsync(
        ProductQueryDto query,
        CancellationToken cancellationToken = default);

    Task<ProductDto> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<ProductDto> CreateAsync(
        CreateProductDto dto,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        int id,
        UpdateProductDto dto,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}