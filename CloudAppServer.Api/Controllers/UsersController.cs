using CloudAppServer.Application.Features.Users;
using CloudAppServer.Infrastructure.ConfigModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CloudAppServer.Controllers;

[ApiController]
[Route("/api/v1/users/")]
public class UsersController(IMediator mediator, IOptions<JwtConfig> jwtConfig) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> AuthorizeUser(UserLoginCommand loginCommand)
    {
        var token = await mediator.Send(loginCommand);
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest();

        Response.Cookies.Append(
            "access_token",
            token,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(jwtConfig.Value.ExpirationMinutes)
            }
        );
        
        return Ok();
    }
}