public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; }
    public int TotalPages { get;}
    public int Page { get; set; }
    public int PageSize { get; set; }

    public PagedResult(List<T> items, int? page, int? pageSize)
    {
        Items = items;
        Page = page ?? 0;
        PageSize = pageSize ?? 0;

        TotalCount = Items.Count;
        TotalPages = (page == null || pageSize == null)
            ? 0
            : (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
