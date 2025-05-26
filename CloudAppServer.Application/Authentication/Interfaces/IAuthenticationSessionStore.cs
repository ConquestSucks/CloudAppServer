using CloudAppServer.Application.Authentication.Models;

namespace CloudAppServer.Application.Authentication.Interfaces;

public interface IAuthenticationSessionStore
{
    bool TryCreateAuthorizationSession(string username);

    Task<AuthenticationUserResponse> WaitForUserResponse(string username, TimeSpan timeout);

    void ApproveUserAuthorization(string username);

    void DenyUserAuthorization(string username);
}