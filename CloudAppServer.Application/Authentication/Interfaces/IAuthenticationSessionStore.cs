namespace CloudAppServer.Application.Authentication.Interfaces;

public interface IAuthenticationSessionStore
{
    bool TryCreateAuthorizationSession(string username);

    Task<bool> WaitForUserResponse(string username, TimeSpan timeout);

    void ApproveUserAuthorization(string username);

    void DenyUserAuthorization(string username);
}