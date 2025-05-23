namespace CloudAppServer.Application.Interfaces;

public interface IAuthorizationService
{
    bool TryCreateAuthorizationSession(string username);
    
    Task<bool> AuthorizeUser(string username, TimeSpan timeout);

    void ApproveUserAuthorization(string username);
    
    void DenyUserAuthorization(string username);
}