namespace CloudAppServer.SharedKernel.Pagination;

public class PagedIntermediateResult<T>(IQueryable<T> queryable, int totalCount, int pageNumber, int pageSize)
{
    public IQueryable<T> Queryable { get; set; } = queryable;

    public int TotalCount { get; set; } = totalCount;

    public int PageNumber { get; set; } = pageNumber;

    public int PageSize { get; set; } = pageSize;
}