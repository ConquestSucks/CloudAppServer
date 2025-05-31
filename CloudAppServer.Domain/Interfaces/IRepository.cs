using CloudAppServer.SharedKernel.Pagination;

namespace CloudAppServer.Domain.Interfaces;

public interface IRepository<T>
{
    Task<T?> GetByIdAsync(Guid id);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task DeleteWithoutRemoveAsync(T entity);
    IQueryable<T> Query();
    Task<PagedIntermediateResult<T>> ToPagedIntermediateResultAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}