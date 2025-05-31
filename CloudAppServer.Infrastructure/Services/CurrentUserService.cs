using CloudAppServer.Application.Interfaces;
using CloudAppServer.SharedKernel.Constants;
using Microsoft.AspNetCore.Http;

namespace CloudAppServer.Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid? UserId
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;

            var userIdString = user?.FindFirst(ClaimTypes.UserId)?.Value;
            if (string.IsNullOrWhiteSpace(userIdString))
                return null;
            
            var userIdGuid = Guid.Parse(userIdString);
            if (userIdGuid == Guid.Empty)
                return null;

            return userIdGuid;
        }
    }
}