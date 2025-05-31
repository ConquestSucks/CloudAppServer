using CloudAppServer.Domain.Interfaces;
using MediatR;

namespace CloudAppServer.Application.Features.Users;

public class IsUserExistsQuery : IRequest<bool>
{
    public required string UserLogin { get; set; }
}

public class IsUserExistsQueryHandler(IUserRepository userRepository) : IRequestHandler<IsUserExistsQuery, bool>
{
    public async Task<bool> Handle(IsUserExistsQuery request, CancellationToken cancellationToken)
    {
        return await userRepository.IsUserExists(request.UserLogin);
    }
}