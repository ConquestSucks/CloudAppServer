using CloudAppServer.Domain.Interfaces;
using CloudAppServer.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace CloudAppServer.Infrastructure.Persistence.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly CloudAppDbContext DbContext;
    private readonly DbSet<T> _dbSet;

    public Repository(CloudAppDbContext dbContext)
    {
        DbContext = dbContext;
        _dbSet = DbContext.Set<T>();
    }
    
    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task AddAsync(T entity)
    {
        if (entity is BaseEntity baseEntity)
        {
            var now = DateTime.UtcNow;
            
            baseEntity.CreatedAt = now;
            baseEntity.ModifiedAt = now;
        }
        
        await _dbSet.AddAsync(entity);
        await DbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        if (entity is BaseEntity baseEntity)
        {
            var now = DateTime.UtcNow;
            
            baseEntity.ModifiedAt = now;
        }
        
        _dbSet.Update(entity);
        await DbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await DbContext.SaveChangesAsync();
    }

    public async Task DeleteWithoutRemoveAsync(T entity)
    {
        if (entity is not BaseEntity baseEntity)
            return;

        baseEntity.IsDeleted = true;
        baseEntity.DeletedAt = DateTime.UtcNow;

        await DbContext.SaveChangesAsync();
    }
}