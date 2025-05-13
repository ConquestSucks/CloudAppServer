using CloudApp.Domain.ValueObjects;
using CloudApp.SharedKernel.Abstractions;

namespace CloudApp.Domain.Entities;

public class User : BaseEntity
{
    public required Email Email { get; set; }
    
    public decimal FreeDiskSpace { get; set; }
    
    public decimal DiskSpaceLeft { get; set; }

    public ICollection<CloudFile> CloudFiles { get; set; } = new List<CloudFile>();

    public ICollection<CloudFolder> CloudFolders { get; set; } = new List<CloudFolder>();
}