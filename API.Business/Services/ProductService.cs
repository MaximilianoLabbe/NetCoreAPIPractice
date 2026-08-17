using API.Business.DTOs;
using API.Business.Enums;
using API.Business.Exceptions;
using API.Business.Interfaces;
using API.Data.Entities;
using API.Data.Enums;
using API.Data.Exceptions;
using API.Data.Queries;
using API.Data.Repositories.Interfaces;
using AutoMapper;

namespace API.Business.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public ProductService(
        IProductRepository productRepository,
        IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<PagedResultDto<ProductDto>> GetAllAsync(
        ProductQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var queryOptions = new ProductQueryOptions
        {
            Name = query.Name,
            MinPrice = query.MinPrice,
            MaxPrice = query.MaxPrice,
            Page = query.Page,
            PageSize = query.PageSize,

            SortBy = query.SortBy switch
            {
                ProductSortField.Name => ProductSortColumn.Name,
                ProductSortField.Price => ProductSortColumn.Price,
                ProductSortField.Stock => ProductSortColumn.Stock,
                ProductSortField.CreatedAt => ProductSortColumn.CreatedAt,
                _ => ProductSortColumn.Id
            },

            SortOrder = query.SortDirection == SortDirection.Desc
                ? SortOrder.Desc
                : SortOrder.Asc
        };

        var result = await _productRepository.GetAllAsync(
            queryOptions,
            cancellationToken);

        var products = _mapper.Map<IReadOnlyCollection<ProductDto>>(
            result.Items);

        return new PagedResultDto<ProductDto>
        {
            TotalRecords = result.TotalRecords,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(
                result.TotalRecords / (double)query.PageSize),
            Items = products
        };
    }

    public async Task<ProductDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(
            id,
            cancellationToken);

        return product is null
            ? null
            : _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> CreateAsync(
    CreateProductDto dto,
    CancellationToken cancellationToken = default)
    {
        var exists = await _productRepository.ExistsByNameAsync(
            dto.Name,
            cancellationToken: cancellationToken);

        if (exists)
        {
            throw new DuplicateResourceException(
                nameof(Product),
                nameof(Product.Name),
                dto.Name);
        }

        try
        {
            var product = _mapper.Map<Product>(dto);

            var createdProduct = await _productRepository.CreateAsync(
                product,
                cancellationToken);

            return _mapper.Map<ProductDto>(createdProduct);
        }
        catch (DuplicateKeyException)
        {
            throw new DuplicateResourceException(
                nameof(Product),
                nameof(Product.Name),
                dto.Name);
        }
    }

    public async Task<bool> UpdateAsync(
     int id,
     UpdateProductDto dto,
     CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (product is null)
        {
            return false;
        }

        var nameExists = await _productRepository.ExistsByNameAsync(
            dto.Name,
            excludeId: id,
            cancellationToken);

        if (nameExists)
        {
            throw new DuplicateResourceException(
                nameof(Product),
                nameof(Product.Name),
                dto.Name);
        }

        _mapper.Map(dto, product);

        try
        {
            await _productRepository.UpdateAsync(
                product,
                cancellationToken);
        }
        catch (DuplicateKeyException)
        {
            throw new DuplicateResourceException(
                nameof(Product),
                nameof(Product.Name),
                dto.Name);
        }

        return true;
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (product is null)
        {
            return false;
        }

        await _productRepository.DeleteAsync(
            product,
            cancellationToken);

        return true;
    }
}