using CloudAppServer.SharedKernel.Abstractions;

namespace CloudAppServer.Domain.Entities;

public class User : BaseEntity
{
    public required string DisplayName { get; set; }
    
    public long TelegramChatId { get; set; }
    
    public decimal DiskSpace { get; set; }
    
    public string? Avatar { get; set; }

    public ICollection<CloudFile> CloudFiles { get; set; } = new List<CloudFile>();

    public ICollection<CloudFolder> CloudFolders { get; set; } = new List<CloudFolder>();
}