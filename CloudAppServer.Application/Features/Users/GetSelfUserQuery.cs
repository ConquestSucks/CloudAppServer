using CloudAppServer.Application.DTOs;
using CloudAppServer.Application.Exceptions;
using CloudAppServer.Application.Interfaces;
using CloudAppServer.Domain.Interfaces;
using MediatR;

namespace CloudAppServer.Application.Features.Users;

public class GetSelfUserQuery : IRequest<UserDto>;

public class GetSelfUserQueryHandler(
    IUserRepository userRepository,
    ICurrentUserService currentUserService) : IRequestHandler<GetSelfUserQuery, UserDto>
{
    public async Task<UserDto> Handle(GetSelfUserQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (userId is null)
            throw new NotFoundException("Пользователь не найден");
        
        var user = await userRepository.GetByIdAsync(userId.Value);
        if (user is null)
            throw new NotFoundException("Пользователь не найден");

        var freeDiskSpace = await userRepository.GetUserFreeDiskSpace(userId.Value);
        var diskSpaceOccupied = await userRepository.GetUserDiskSpaceOccupied(userId.Value);
        
        return new UserDto
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            Name = user.Name,
            FreeDiskSpace = freeDiskSpace,
            DiskSpaceOccupied = diskSpaceOccupied,
            DiskSpace = user.DiskSpace,
            Avatar = user.Avatar,
            CreatedAt = user.CreatedAt,
            ModifiedAt = user.ModifiedAt
        };
    }
}