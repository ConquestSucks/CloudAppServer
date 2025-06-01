namespace CloudAppServer.Application.DTOs;

public class QuotaItemDto
{
    public required string FileType { get; set; }
    
    public decimal TotalSize { get; set; }
}