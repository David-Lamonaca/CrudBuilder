public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; }
    public int TotalPages { get; }
    public int Page { get; set; }
    public int PageSize { get; set; }

    public PagedResult(List<T> items, int totalCount, int? page, int? pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page ?? 1;
        PageSize = pageSize ?? totalCount;

        TotalPages = pageSize.HasValue && pageSize > 0
            ? (int)Math.Ceiling((double)TotalCount / PageSize)
            : 1;
    }
}
