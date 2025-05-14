using CloudAppServer.Application.Interfaces;
using CloudAppServer.Domain.Interfaces;
using MediatR;

namespace CloudAppServer.Application.Features.Users;

public class AuthenticateUserCommand : IRequest<bool>
{
    public required string Login { get; set; }
}

public class AuthenticateUserCommandHandler(IUserRepository repository, ITelegramService telegramService) 
    : IRequestHandler<AuthenticateUserCommand, bool>
{
    public async Task<bool> Handle(AuthenticateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await repository.FindUserByNameAsync(request.Login);
        if (user is null)
            return false;
        
        await telegramService.SendLoginRequestAsync(user);

        return true;
    }
}