using CloudAppServer.Domain.Entities;
using CloudAppServer.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CloudAppServer.Infrastructure.Persistence.Repositories;

public class CloudFileRepository(CloudAppDbContext dbContext) : Repository<CloudFile>(dbContext), ICloudFileRepository
{
    public async Task<CloudFile?> GetFileByKeyAsync(string key, Guid userId)
    {
        return await DbContext.CloudFiles
            .Include(f => f.User)
            .FirstOrDefaultAsync(f => f.Key == key 
                                      && !f.IsDeleted
                                      && f.UserId == userId);
    }

    public async Task<CloudFile?> GetDeletedFileByKeyAsync(string key, Guid userId)
    {
        return await DbContext.CloudFiles
            .Include(f => f.User)
            .FirstOrDefaultAsync(f => f.Key == key 
                                      && f.IsDeleted 
                                      && f.UserId == userId);
    }
}