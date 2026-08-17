namespace API.Data.Models;

public class PagedDataResult<T>
{
    public int TotalRecords { get; set; }

    public IReadOnlyCollection<T> Items { get; set; } = [];
}