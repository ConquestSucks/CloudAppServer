namespace CloudAppServer.SharedKernel.Pagination;

public class PagedResult<T>(
    IReadOnlyList<T> items,
    int totalCount,
    int pageNumber,
    int pageSize)
{
    /// <summary>Элементы текущей страницы</summary>
    public IReadOnlyList<T> Items { get; init; } = items;

    /// <summary>Общее число записей во всём наборе</summary>
    public int TotalCount { get; init; } = totalCount;

    /// <summary>Номер текущей страницы (начиная с 1)</summary>
    public int PageNumber { get; init; } = pageNumber;

    /// <summary>Размер страницы (сколько элементов в каждой)</summary>
    public int PageSize { get; init; } = pageSize;

    /// <summary>Общее количество страниц на основе TotalCount и PageSize</summary>
    public int TotalPages
    {
        get
        {
            if (PageSize <= 0)
                return 0;

            var pages = TotalCount / PageSize;
            if (TotalCount % PageSize > 0)
                pages++;
            return pages;
        }
    }
}