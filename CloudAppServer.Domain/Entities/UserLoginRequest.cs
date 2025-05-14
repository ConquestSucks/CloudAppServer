using CloudAppServer.Domain.Enums;
using CloudAppServer.SharedKernel.Abstractions;

namespace CloudAppServer.Domain.Entities;

public class UserLoginRequest : BaseEntity
{
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;
    
    public LoginRequestStatus LoginRequestStatus { get; private set; }
    
    public void Approve() => LoginRequestStatus = LoginRequestStatus.Approved;
    
    public void Deny() => LoginRequestStatus = LoginRequestStatus.Denied;
}