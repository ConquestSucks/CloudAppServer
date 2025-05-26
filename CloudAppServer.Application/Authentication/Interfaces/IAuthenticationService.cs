using CloudAppServer.Domain.Entities;

namespace CloudAppServer.Application.Authentication.Interfaces;

public interface IAuthenticationService
{
    Task<bool> TrySendLoginRequestAndWaitAsync(User user, TimeSpan timeout);
}