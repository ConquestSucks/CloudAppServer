using CloudAppServer.Application.Exceptions;
using CloudAppServer.Application.Interfaces;
using CloudAppServer.Domain.Interfaces;
using MediatR;

namespace CloudAppServer.Application.Features.CloudFiles;

public class DeleteFileWithoutRemoveCommand : IRequest
{
    public required string Key { get; set; }
}

public class DeleteFileWithoutRemoveCommandHandler(
    IS3Service s3Service, 
    ICurrentUserService currentUserService,
    ICloudFileRepository cloudFileRepository) : IRequestHandler<DeleteFileWithoutRemoveCommand>
{
    public async Task Handle(DeleteFileWithoutRemoveCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (userId is null)
            throw new NotFoundException("Пользователь не найден");
        
        if (string.IsNullOrEmpty(request.Key))
            throw new NotFoundException("Пустое название файла");

        var cloudFile = await cloudFileRepository.GetFileByKeyAsync(request.Key, userId.Value);
        if (cloudFile is null)
            throw new NotFoundException("Такого файла не существует");

        await cloudFileRepository.DeleteWithoutRemoveAsync(cloudFile);
    }
}