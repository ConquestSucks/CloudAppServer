namespace CloudAppServer.Application.DTOs;

public class CloudFolderDto
{
    public Guid Id { get; set; }
    
    public Guid? ParentCloudFolderId { get; set; }
    
    public string? PublicUrl { get; set; }
    
    public required string Name { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime ModifiedAt { get; set; }
    
    public List<CloudFolderDto> SubFolders { get; set; } = [];
}