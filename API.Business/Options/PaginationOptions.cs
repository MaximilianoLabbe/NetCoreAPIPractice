namespace API.Business.Options;

public class PaginationOptions
{
    public const string SectionName = "Pagination";

    public int DefaultPageSize { get; set; } = 10;
    public int MaxPageSize { get; set; } = 100;
}