using System.Security.Claims;
using CloudAppServer.Application.Authentication.Interfaces;
using CloudAppServer.Application.Exceptions;
using CloudAppServer.Application.Interfaces;
using CloudAppServer.Domain.Interfaces;
using MediatR;
using ClaimTypes = CloudAppServer.SharedKernel.Constants.ClaimTypes;

namespace CloudAppServer.Application.Features.Users;

public class UserLoginCommand : IRequest<string?>
{
    public required string Login { get; set; }
}

public class UserLoginCommandHandler(
    IAuthenticationService authenticationService,
    IUserLoginRequestRepository userLoginRequestRepository,
    IUserRepository userRepository,
    IJwtService jwtService) 
    : IRequestHandler<UserLoginCommand, string?>
{
    public async Task<string?> Handle(UserLoginCommand request, CancellationToken cancellationToken)
    {
        var anyActiveRequests = await userLoginRequestRepository.AnyActiveRequests(request.Login);
        if (anyActiveRequests)
            throw new ConflictException("Аутентификация уже в процессе");
        
        var user = await userRepository.FindUserByNameAsync(request.Login);
        if (user is null)
            throw new NotFoundException("Такого пользователя не существует");
        
        var authenticationCompleted = await authenticationService.TrySendLoginRequestAndWaitAsync(user, 
            TimeSpan.FromMinutes(2));
        if (!authenticationCompleted)
            throw new ConflictException("Неизвестная ошибка аутентификации");

        return jwtService.CreateToken([
            new Claim(ClaimTypes.UserId, user.Id.ToString())
        ]);
    }
}