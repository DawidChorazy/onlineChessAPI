namespace OnlineChessAPI.Core.DTOs;

public class PagedListDto<T> where T : class
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
    
    public Dictionary<string, string> Links { get; set; } = new Dictionary<string, string>();

    public PagedListDto(IEnumerable<T> items, int totalCount, int pageSize, int currentPage)
    {
        Items = items;
        TotalCount = totalCount;
        PageSize = pageSize;
        CurrentPage = currentPage;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }
}
