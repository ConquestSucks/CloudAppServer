using CloudAppServer.Application.DTOs;
using CloudAppServer.Application.Exceptions;
using CloudAppServer.Application.Interfaces;
using CloudAppServer.Domain.Interfaces;
using MediatR;

namespace CloudAppServer.Application.Features.CloudFiles;

public class RestoreFileCommand : IRequest<CloudFileDto>
{
    public required string Key { get; init; }
}

public class RestoreFileCommandHandler(
    ICloudFileRepository cloudFileRepository,
    ICurrentUserService currentUserService) : IRequestHandler<RestoreFileCommand, CloudFileDto>
{
    public async Task<CloudFileDto> Handle(RestoreFileCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (userId is null)
            throw new NotFoundException("Пользователь не найден");

        var cloudFile = await cloudFileRepository.GetDeletedFileByKeyAsync(request.Key, userId.Value);
        if (cloudFile is null)
            throw new NotFoundException("Такого файла не существует");
        
        cloudFile.Restore();
        await cloudFileRepository.UpdateAsync(cloudFile);

        return new CloudFileDto
        {
            Id = cloudFile.Id,
            UserDisplayName = cloudFile.User.DisplayName,
            CloudFolderId = cloudFile.CloudFolderId,
            PublicUrl = cloudFile.PublicUrl?.Value,
            Key = cloudFile.Key,
            Size = cloudFile.Size,
            Name = cloudFile.Name,
            Extension = cloudFile.Extension,
            DownloadCount = cloudFile.DownloadCount,
            CreatedAt = cloudFile.CreatedAt,
            ModifiedAt = cloudFile.ModifiedAt,
            DeletedAt = cloudFile.DeletedAt
        };
    }
}