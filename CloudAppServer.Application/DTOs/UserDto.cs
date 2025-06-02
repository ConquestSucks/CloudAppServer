namespace CloudAppServer.Application.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    
    public required string DisplayName { get; set; }
    
    public required string Name { get; set; }
    
    public decimal FreeDiskSpace { get; set; }
    
    public decimal DiskSpaceOccupied { get; set; }
    
    public decimal DiskSpace { get; set; }
    
    public string? Avatar { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime ModifiedAt { get; set; }
}