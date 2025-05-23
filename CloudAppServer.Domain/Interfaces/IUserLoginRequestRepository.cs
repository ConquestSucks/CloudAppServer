namespace CloudAppServer.Domain.Interfaces;

public interface IUserLoginRequestRepository
{
    Task<bool> AnyActiveRequests(string username);
}