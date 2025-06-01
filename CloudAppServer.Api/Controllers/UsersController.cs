using CloudAppServer.Application.DTOs;
using CloudAppServer.Application.Features.CloudFiles;
using CloudAppServer.Application.Features.Users;
using CloudAppServer.Infrastructure.ConfigModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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

    [HttpGet("isUserLoggedIn")]
    [Authorize]
    public IActionResult IsUserLoggedIn()
    {
        return Ok();
    }

    [HttpGet("getSelfUser")]
    [Authorize]
    public async Task<UserDto> GetSelfUser()
    {
        return await mediator.Send(new GetSelfUserQuery());
    }

    [HttpGet("isUserExists")]
    public async Task<bool> IsUserExists(string userLogin)
    {
        return await mediator.Send(new IsUserExistsQuery
        {
            UserLogin = userLogin
        });
    }

    [HttpGet("getUserQuota")]
    [Authorize]
    public async Task<List<QuotaItemDto>> GetUserQuota()
    {
        return await mediator.Send(new GetUserQuotaQuery());
    }
}