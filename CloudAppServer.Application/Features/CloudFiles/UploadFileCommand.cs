using CloudAppServer.Application.Exceptions;
using CloudAppServer.Application.Interfaces;
using CloudAppServer.Domain.Entities;
using CloudAppServer.Domain.Interfaces;
using CloudAppServer.SharedKernel.Interfaces;
using MediatR;

namespace CloudAppServer.Application.Features.CloudFiles;

public class UploadFileCommand : IRequest
{
    public required string Key { get; init; }
    
    public required Stream Stream { get; init; }
    
    public Guid? CloudFolderId { get; init; }
    
    public required string ConnectionId { get; set; }
}

public class UploadFileCommandHandler(
    IS3Service s3Service, 
    ICloudFileRepository cloudFileRepository,
    ICurrentUserService currentUserService,
    IUserRepository userRepository,
    IFileUploadNotifier fileUploadNotifier) : IRequestHandler<UploadFileCommand>
{
    public async Task Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (userId is null)
            throw new NotFoundException("Пользователь не найден");
        
        var freeDiskSpace = await userRepository.GetUserFreeDiskSpace(userId.Value);
        if (request.Stream.Length > freeDiskSpace)
            throw new NotEnoughDiskSpaceException();
        
        var fileExtension = request.Key.Split('.').LastOrDefault();
        if (string.IsNullOrWhiteSpace(fileExtension))
            throw new InternalServerErrorException("Не удалось распознать расширение файла");
        
        var fileName = request.Key.Replace($".{fileExtension}", "");
        if (string.IsNullOrWhiteSpace(fileName))
            throw new InternalServerErrorException("Не удалось распознать название файла");

        if (string.IsNullOrWhiteSpace(request.ConnectionId))
            throw new ConflictException("Номер подключения не передан");

        var size = request.Stream.Length;
        var key = Guid.NewGuid();
        var cloudFile = new CloudFile
        {
            UserId = userId.Value,
            CloudFolderId = request.CloudFolderId,
            Key = key.ToString(),
            Size = 0,
            Extension = fileExtension,
            Name = fileName
        };

        await cloudFileRepository.AddAsync(cloudFile);

        try
        {
            await s3Service.UploadFileAsync(key.ToString(), request.Stream,
                percent => { _ = fileUploadNotifier.NotifyProgressAsync(request.ConnectionId, percent); },
                cancellationToken);
            
            if (request.Stream.CanSeek)
            {
                cloudFile.Size = size;
                
                await cloudFileRepository.UpdateAsync(cloudFile);
            }
        }
        catch (OperationCanceledException)
        {
            await cloudFileRepository.DeleteAsync(cloudFile);

            throw;
        }
        catch (Exception)
        {
            await cloudFileRepository.DeleteAsync(cloudFile);

            throw;
        }
    }
}