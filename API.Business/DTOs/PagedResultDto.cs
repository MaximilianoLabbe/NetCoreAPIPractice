namespace API.Business.DTOs;

public class PagedResultDto<T>
{
    public int TotalRecords { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalPages { get; set; }

    public IReadOnlyCollection<T> Items { get; set; } = [];
}