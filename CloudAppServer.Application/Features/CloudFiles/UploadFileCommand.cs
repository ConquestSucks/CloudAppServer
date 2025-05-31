using CloudAppServer.Application.Exceptions;
using CloudAppServer.Application.Interfaces;
using CloudAppServer.Domain.Entities;
using CloudAppServer.Domain.Interfaces;
using MediatR;

namespace CloudAppServer.Application.Features.CloudFiles;

public class UploadFileCommand : IRequest
{
    public required string Key { get; init; }
    
    public required Stream Stream { get; init; }
    
    public Guid? CloudFolderId { get; init; }
}

public class UploadFileCommandHandler(
    IS3Service s3Service, 
    ICloudFileRepository cloudFileRepository,
    ICurrentUserService currentUserService,
    IUserRepository userRepository) : IRequestHandler<UploadFileCommand>
{
    public async Task Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (userId is null)
            throw new NotFoundException("Пользователь не найден");

        var freeDiskSpace = await userRepository.GetUserFreeDiskSpace(userId.Value);
        if (request.Stream.Length > freeDiskSpace)
            throw new NotEnoughDiskSpace();
        
        var fileExtension = request.Key.Split('.').LastOrDefault();
        if (string.IsNullOrWhiteSpace(fileExtension))
            throw new InternalServerErrorException("Не удалось распознать расширение файла");
        
        var fileName = request.Key.Replace($".{fileExtension}", "");
        if (string.IsNullOrWhiteSpace(fileName))
            throw new InternalServerErrorException("Не удалось распознать название файла");
        
        var key = Guid.NewGuid();
        var cloudFile = new CloudFile
        {
            UserId = userId.Value,
            CloudFolderId = request.CloudFolderId,
            Key = key.ToString(),
            Size = request.Stream.Length,
            Extension = fileExtension,
            Name = fileName
        };

        await cloudFileRepository.AddAsync(cloudFile);
        
        await s3Service.UploadFileAsync(key.ToString(), request.Stream);
    }
}