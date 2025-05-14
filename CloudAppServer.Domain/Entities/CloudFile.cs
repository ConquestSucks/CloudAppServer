using CloudAppServer.Domain.ValueObjects;
using CloudAppServer.SharedKernel.Abstractions;

namespace CloudAppServer.Domain.Entities;

public class CloudFile : BaseEntity
{
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;
    
    public Guid? CloudFolderId { get; set; }
    
    public CloudFolder? CloudFolder { get; set; }
    
    public PublicUrl? PublicUrl { get; set; }
    
    public required decimal Size { get; set; }
    
    public required string Extension { get; set; }
    
    public int DownloadCount { get; set; }
}