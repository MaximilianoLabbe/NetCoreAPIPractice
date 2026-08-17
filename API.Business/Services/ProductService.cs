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
using API.Business.Options;
using Microsoft.Extensions.Options;

namespace API.Business.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly PaginationOptions _paginationOptions;
    public ProductService(
        IProductRepository productRepository,
        IMapper mapper,
        IOptions<PaginationOptions> paginationOptions)
    {
        _productRepository = productRepository;
        _mapper = mapper;
        _paginationOptions = paginationOptions.Value;
    }

    public async Task<PagedResultDto<ProductDto>> GetAllAsync(
        ProductQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var pageSize = query.PageSize
    ?? _paginationOptions.DefaultPageSize;

        pageSize = Math.Min(
            pageSize,
            _paginationOptions.MaxPageSize);

        var queryOptions = new ProductQueryOptions
        {
            Name = query.Name,
            MinPrice = query.MinPrice,
            MaxPrice = query.MaxPrice,
            Page = query.Page,
            PageSize = pageSize,

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
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(
                result.TotalRecords / (double)pageSize),
            Items = products
        };
    }

    public async Task<ProductDto> GetByIdAsync(
    int id,
    CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (product is null)
        {
            throw new ResourceNotFoundException(
                nameof(Product),
                id);
        }

        return _mapper.Map<ProductDto>(product);
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

    public async Task UpdateAsync(
        int id,
        UpdateProductDto dto,
        CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (product is null)
        {
            throw new ResourceNotFoundException(
                nameof(Product),
                id);
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
    }

    public async Task DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (product is null)
        {
            throw new ResourceNotFoundException(
                nameof(Product),
                id);
        }

        await _productRepository.DeleteAsync(
            product,
            cancellationToken);
    }
}