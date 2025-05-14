using CloudAppServer.SharedKernel.Abstractions;

namespace CloudAppServer.Domain.Entities;

public class User : BaseEntity
{
    public long TelegramChatId { get; set; }
    
    public decimal FreeDiskSpace { get; set; }
    
    public decimal DiskSpaceLeft { get; set; }

    public ICollection<CloudFile> CloudFiles { get; set; } = new List<CloudFile>();

    public ICollection<CloudFolder> CloudFolders { get; set; } = new List<CloudFolder>();
}