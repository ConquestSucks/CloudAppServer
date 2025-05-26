using CloudAppServer.Domain.Entities;

namespace CloudAppServer.Domain.Interfaces;

public interface IUserLoginRequestRepository : IRepository<UserLoginRequest>
{
    Task<bool> AnyActiveRequests(string username);
}