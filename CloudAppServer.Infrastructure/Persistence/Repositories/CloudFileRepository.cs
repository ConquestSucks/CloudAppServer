using CloudAppServer.Domain.Entities;
using CloudAppServer.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CloudAppServer.Infrastructure.Persistence.Repositories;

public class CloudFileRepository(CloudAppDbContext dbContext) : Repository<CloudFile>(dbContext), ICloudFileRepository
{
    public async Task<CloudFile?> GetByKeyAsync(string key)
    {
        return await DbContext.CloudFiles.FirstOrDefaultAsync(f => f.Key == key);
    }
}