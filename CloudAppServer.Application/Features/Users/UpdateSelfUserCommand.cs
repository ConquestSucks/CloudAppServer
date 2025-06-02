using CloudAppServer.Application.DTOs;
using CloudAppServer.Application.Exceptions;
using CloudAppServer.Application.Interfaces;
using CloudAppServer.Domain.Interfaces;
using MediatR;

namespace CloudAppServer.Application.Features.Users;

public class UpdateSelfUserCommand : IRequest<UserDto>
{
    public required string DisplayName { get; set; }
    
    public Stream? AvatarStream { get; set; }
    
    public string? AvatarFileName { get; set; }
}

public class UpdateSelfUserCommandHandler(
    ICurrentUserService currentUserService,
    IUserRepository userRepository) : IRequestHandler<UpdateSelfUserCommand, UserDto>
{
    public async Task<UserDto> Handle(UpdateSelfUserCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (userId is null)
            throw new NotFoundException("Пользователь не найден");

        var user = await userRepository.GetByIdAsync(userId.Value);
        if (user is null)
            throw new NotFoundException("Пользователь не найден");

        if (request.AvatarStream is not null && !string.IsNullOrWhiteSpace(request.AvatarFileName))
        {
            var now = DateTime.UtcNow;
            var extension = Path.GetExtension(request.AvatarFileName);
            var avatarName = $"{user.Id}-{now:yyyyMMddHHmmss}{extension}";
            var avatarsFolderPath = Path.Combine(Environment.CurrentDirectory, "avatars");
            var avatarPath = Path.Combine(avatarsFolderPath, avatarName);
            
            if (!Directory.Exists(avatarsFolderPath))
                Directory.CreateDirectory(avatarsFolderPath);

            await using var stream = new FileStream(avatarPath, FileMode.Create);

            await request.AvatarStream.CopyToAsync(stream, cancellationToken);

            user.Avatar = avatarName;
        }

        user.DisplayName = request.DisplayName;

        await userRepository.UpdateAsync(user);
        
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