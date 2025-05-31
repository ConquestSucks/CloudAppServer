using CloudAppServer.Application.Exceptions;
using CloudAppServer.Application.Interfaces;
using CloudAppServer.Domain.Interfaces;
using MediatR;

namespace CloudAppServer.Application.Features.CloudFiles;

public class DownloadFileQuery : IRequest<(Stream stream, string contentType, string fileName)>
{
    public required string Key { get; init; }
}

public class DownloadFileQueryHandler(
    IS3Service s3Service, 
    ICurrentUserService currentUserService,
    ICloudFileRepository cloudFileRepository) 
    : IRequestHandler<DownloadFileQuery, (Stream stream, string contentType, string fileName)>
{
    public async Task<(Stream stream, string contentType, string fileName)> Handle(DownloadFileQuery request, 
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (userId is null)
            throw new NotFoundException("Пользователь не найден");
        
        if (string.IsNullOrEmpty(request.Key))
            throw new NotFoundException("Пустое название файла");

        var cloudFile = await cloudFileRepository.GetByKeyAsync(request.Key);
        if (cloudFile is null)
            throw new NotFoundException("Такого файла не существует");

        if (cloudFile.UserId != userId.Value)
            throw new ForbiddenException("");
        
        var stream = await s3Service.DownloadFileAsync(request.Key);
        
        return (stream, "application/octet-stream", cloudFile.DisplayName);
    }
}