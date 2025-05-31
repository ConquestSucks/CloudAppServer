using CloudAppServer.Application.DTOs;
using CloudAppServer.Application.Exceptions;
using CloudAppServer.Application.Interfaces;
using CloudAppServer.Domain.Interfaces;
using MediatR;

namespace CloudAppServer.Application.Features.CloudFiles;

public class GetUserFileQuery : IRequest<CloudFileDto>
{
    public required string Key { get; init; }
    
    public bool DeletedFile { get; init; }
}

public class GetUserFileQueryHandler(
    ICurrentUserService currentUserService,
    ICloudFileRepository cloudFileRepository) : IRequestHandler<GetUserFileQuery, CloudFileDto>
{
    public async Task<CloudFileDto> Handle(GetUserFileQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (userId is null)
            throw new NotFoundException("Пользователь не найден");
        
        var cloudFile = request.DeletedFile ? 
            await cloudFileRepository.GetDeletedFileByKeyAsync(request.Key, userId.Value)
            : await cloudFileRepository.GetFileByKeyAsync(request.Key, userId.Value);
        if (cloudFile is null)
            throw new NotFoundException("Такого файла не существует");

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