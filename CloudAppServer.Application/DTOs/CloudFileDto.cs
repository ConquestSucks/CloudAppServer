namespace CloudAppServer.Application.DTOs;

public class CloudFileDto
{
    public Guid Id { get; set; }
    
    public required string UserDisplayName { get; set; }
    
    public Guid? CloudFolderId { get; set; }
    
    public string? PublicUrl { get; set; }
    
    public required string Key { get; set; }
    
    public required decimal Size { get; set; }
    
    public required string Name { get; set; }
    
    public required string Extension { get; set; }
    
    public int DownloadCount { get; set; }
    
    public required DateTime CreatedAt { get; set; }
    
    public required DateTime ModifiedAt { get; set; }
    
    public required DateTime? DeletedAt { get; set; }
}