using System.Security.Claims;

namespace CloudAppServer.Application.Interfaces;

public interface IJwtService
{
    string CreateToken(IEnumerable<Claim> claims);
}