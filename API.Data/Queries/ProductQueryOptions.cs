using API.Data.Enums;

namespace API.Data.Queries;

public class ProductQueryOptions
{
    public string? Name { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }

    public ProductSortColumn SortBy { get; set; }

    public SortOrder SortOrder { get; set; }
}