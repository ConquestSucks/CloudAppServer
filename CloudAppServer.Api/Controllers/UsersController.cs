using CloudAppServer.Application.Features.Users;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CloudAppServer.Controllers;

[ApiController]
[Route("/api/v1/users")]
public class UsersController(IMediator mediator) : ControllerBase
{
    [HttpPost("authenticate")]
    public async Task<IActionResult> AuthenticateUser(AuthenticateUserCommand command)
    {
        var result = await mediator.Send(command);

        return result ? Ok() : NotFound();
    }
}