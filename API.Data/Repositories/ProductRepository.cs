using API.Data.Context;
using API.Data.Entities;
using API.Data.Enums;
using API.Data.Constants;
using API.Data.Exceptions;
using API.Data.Models;
using API.Data.Queries;
using API.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using SqlException = Microsoft.Data.SqlClient.SqlException;
namespace API.Data.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedDataResult<Product>> GetAllAsync(
        ProductQueryOptions query,
        CancellationToken cancellationToken = default)
    {
        var productsQuery = _context.Products
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            productsQuery = productsQuery.Where(
                p => p.Name.Contains(query.Name));
        }

        if (query.MinPrice.HasValue)
        {
            productsQuery = productsQuery.Where(
                p => p.Price >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            productsQuery = productsQuery.Where(
                p => p.Price <= query.MaxPrice.Value);
        }

        var totalRecords = await productsQuery
            .CountAsync(cancellationToken);

        productsQuery = query.SortBy switch
        {
            ProductSortColumn.Name => query.SortOrder == SortOrder.Desc
                ? productsQuery.OrderByDescending(p => p.Name)
                : productsQuery.OrderBy(p => p.Name),

            ProductSortColumn.Price => query.SortOrder == SortOrder.Desc
                ? productsQuery.OrderByDescending(p => p.Price)
                : productsQuery.OrderBy(p => p.Price),

            ProductSortColumn.Stock => query.SortOrder == SortOrder.Desc
                ? productsQuery.OrderByDescending(p => p.Stock)
                : productsQuery.OrderBy(p => p.Stock),

            ProductSortColumn.CreatedAt => query.SortOrder == SortOrder.Desc
                ? productsQuery.OrderByDescending(p => p.CreatedAt)
                : productsQuery.OrderBy(p => p.CreatedAt),

            ProductSortColumn.Id => query.SortOrder == SortOrder.Desc
                ? productsQuery.OrderByDescending(p => p.Id)
                : productsQuery.OrderBy(p => p.Id),

            _ => productsQuery.OrderBy(p => p.Id)
        };

        var products = await productsQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedDataResult<Product>
        {
            TotalRecords = totalRecords,
            Items = products
        };
    }

    public async Task<Product?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.Id == id,
                cancellationToken);
    }

    public async Task<Product> CreateAsync(
     Product product,
     CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Products.AddAsync(
                product,
                cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return product;
        }
        catch (DbUpdateException exception)
            when (IsUniqueConstraintViolation(exception))
        {
            throw new DuplicateKeyException(
                nameof(Product),
                nameof(Product.Name),
                product.Name,
                exception);
        }
    }

    public async Task UpdateAsync(
    Product product,
    CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Products.Update(product);

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (IsUniqueConstraintViolation(exception))
        {
            throw new DuplicateKeyException(
                nameof(Product),
                nameof(Product.Name),
                product.Name,
                exception);
        }
    }

    public async Task DeleteAsync(
        Product product,
        CancellationToken cancellationToken = default)
    {
        _context.Products.Remove(product);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(
    string name,
    int? excludeId = null,
    CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AsNoTracking()
            .AnyAsync(
                p => p.Name == name &&
                     (!excludeId.HasValue || p.Id != excludeId.Value),
                cancellationToken);
    }

    private static bool IsUniqueConstraintViolation(
    DbUpdateException exception)
    {
        return exception.InnerException is SqlException sqlException
            && sqlException.Number is
                SqlServerErrorNumbers.DuplicateKey or
                SqlServerErrorNumbers.UniqueConstraintViolation;
    }
}