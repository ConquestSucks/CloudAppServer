using CloudAppServer.Domain.ValueObjects;
using CloudAppServer.SharedKernel.Abstractions;

namespace CloudAppServer.Domain.Entities;

public class CloudFolder : BaseEntity
{
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public Guid? ParentCloudFolderId { get; set; }
    
    public CloudFolder? ParentCloudFolder { get; set; }
    
    public PublicUrl? PublicUrl { get; set; }
    
    public ICollection<CloudFolder> SubFolders { get; set; } = new List<CloudFolder>();
}