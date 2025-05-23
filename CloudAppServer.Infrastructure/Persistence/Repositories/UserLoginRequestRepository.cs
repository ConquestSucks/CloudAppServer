using CloudAppServer.Domain.Entities;
using CloudAppServer.Domain.Enums;
using CloudAppServer.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CloudAppServer.Infrastructure.Persistence.Repositories;

public class UserLoginRequestRepository(CloudAppDbContext dbContext) : Repository<UserLoginRequest>(dbContext), 
    IUserLoginRequestRepository
{
    public Task<bool> AnyActiveRequests(string username)
    {
        return DbContext.UserLoginRequests
            .Include(r => r.User)
            .AnyAsync(r => r.User.Name == username 
                           && r.LoginRequestStatus == LoginRequestStatus.None 
                           && !r.IsDeleted);
    }
}