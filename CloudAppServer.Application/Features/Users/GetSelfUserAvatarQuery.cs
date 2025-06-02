using CloudAppServer.Application.Exceptions;
using CloudAppServer.Application.Interfaces;
using CloudAppServer.Domain.Interfaces;
using MediatR;

namespace CloudAppServer.Application.Features.Users;

public class GetSelfUserAvatarQuery : IRequest<byte[]>;

public class GetSelfUserAvatarQueryHandler(
    ICurrentUserService currentUserService,
    IUserRepository userRepository) : IRequestHandler<GetSelfUserAvatarQuery, byte[]>
{
    public async Task<byte[]> Handle(GetSelfUserAvatarQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (userId is null)
            throw new NotFoundException("Пользователь не найден");

        var user = await userRepository.GetByIdAsync(userId.Value);
        if (user is null)
            throw new NotFoundException("Пользователь не найден");

        if (string.IsNullOrWhiteSpace(user.Avatar))
            throw new NotFoundException();
        
        var avatarsPath = Path.Combine(Environment.CurrentDirectory, "avatars");
        var path = Path.Combine(avatarsPath, user.Avatar);
        
        return await File.ReadAllBytesAsync(path, cancellationToken);
    }
}