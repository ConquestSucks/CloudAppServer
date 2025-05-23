using System.Security.Claims;
using CloudAppServer.Application.Interfaces;
using CloudAppServer.Domain.Interfaces;
using MediatR;
using ClaimTypes = CloudAppServer.SharedKernel.Constants.ClaimTypes;

namespace CloudAppServer.Application.Features.Users;

public class UserLoginCommand : IRequest<string?>
{
    public required string Login { get; set; }
}

public class AuthorizeUserCommandHandler(
    IAuthorizationService authorizationService,
    IUserLoginRequestRepository userLoginRequestRepository,
    IUserRepository userRepository,
    IJwtService jwtService,
    ITelegramService telegramService) 
    : IRequestHandler<UserLoginCommand, string?>
{
    public async Task<string?> Handle(UserLoginCommand request, CancellationToken cancellationToken)
    {
        var anyActiveRequests = await userLoginRequestRepository.AnyActiveRequests(request.Login);
        if (anyActiveRequests)
            return null;
        
        var user = await userRepository.FindUserByNameAsync(request.Login);
        if (user is null)
            return null;
        
        await telegramService.SendLoginRequestAsync(user);

        var authorizationCreated = authorizationService.TryCreateAuthorizationSession(request.Login);
        if (!authorizationCreated)
            return null;

        var authorizationCompleted = await authorizationService.AuthorizeUser(request.Login, 
            TimeSpan.FromMinutes(2));
        if (!authorizationCompleted)
            return null;

        return jwtService.CreateToken([
            new Claim(ClaimTypes.UserId, user.Id.ToString())
        ]);
    }
}