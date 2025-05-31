using CloudAppServer.Domain.Entities;

namespace CloudAppServer.Domain.Interfaces;

public interface ICloudFileRepository : IRepository<CloudFile>
{
    Task<CloudFile?> GetByKeyAsync(string key);
}