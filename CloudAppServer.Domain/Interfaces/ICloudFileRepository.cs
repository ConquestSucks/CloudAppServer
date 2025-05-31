using CloudAppServer.Domain.Entities;

namespace CloudAppServer.Domain.Interfaces;

public interface ICloudFileRepository : IRepository<CloudFile>
{
    Task<CloudFile?> GetFileByKeyAsync(string key, Guid userId);
    
    Task<CloudFile?> GetDeletedFileByKeyAsync(string key, Guid userId);
}