namespace Application.Common.DTOs;

public class PaginatedResult<T> where T : class
{
    public IEnumerable<T> Data { get; set; }

    public int Count { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }
}