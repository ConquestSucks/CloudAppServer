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
                Domain = jwtConfig.Value.Audience,
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

    [HttpPut("updateSelfUser")]
    [Authorize]
    public async Task<UserDto> UpdateSelfUser([FromQuery] string displayName, IFormFile? avatar = null, CancellationToken cancellationToken = default)
    {
        if (avatar is null || avatar.Length <= 0)
            return await mediator.Send(new UpdateSelfUserCommand
            {
                AvatarStream = null,
                DisplayName = displayName
            }, cancellationToken);
        
        await using var stream = avatar.OpenReadStream();

        return await mediator.Send(new UpdateSelfUserCommand
        {
            AvatarStream = stream,
            AvatarFileName = avatar.FileName,
            DisplayName = displayName
        }, cancellationToken);
    }

    [HttpGet("getSelfUserAvatar")]
    [Authorize]
    public async Task<IActionResult> GetSelfUserAvatar(CancellationToken cancellationToken)
    {
        var bytes = await mediator.Send(new GetSelfUserAvatarQuery(), cancellationToken);
        
        return File(bytes, "application/octet-stream");
    }
}