using CloudAppServer.Application.Exceptions;
using CloudAppServer.Application.Interfaces;
using CloudAppServer.Domain.Interfaces;
using MediatR;

namespace CloudAppServer.Application.Features.CloudFiles;

public class DeleteFileCommand : IRequest
{
    public required string Key { get; init; }
}

public class DeleteFileCommandHandler(
    IS3Service s3Service, 
    ICurrentUserService currentUserService,
    ICloudFileRepository cloudFileRepository) : IRequestHandler<DeleteFileCommand>
{
    public async Task Handle(DeleteFileCommand request, CancellationToken cancellationToken)
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

        await cloudFileRepository.DeleteAsync(cloudFile);
        
        await s3Service.DeleteFileAsync(request.Key);
    }
}