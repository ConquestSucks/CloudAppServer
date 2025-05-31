using CloudAppServer.Application.DTOs;
using CloudAppServer.Application.Exceptions;
using CloudAppServer.Application.Interfaces;
using CloudAppServer.Domain.Interfaces;
using CloudAppServer.SharedKernel.Pagination;
using MediatR;

namespace CloudAppServer.Application.Features.CloudFiles;

public class GetUserFilesQuery : IRequest<PagedResult<CloudFileDto>>
{
    public int PageNumber { get; init; }
    
    public int PageSize { get; init; }
    
    public bool DeletedFiles { get; init; }
}

public class GetUserFilesQueryHandler(
    ICloudFileRepository cloudFileRepository,
    ICurrentUserService currentUserService,
    IUserRepository userRepository) 
    : IRequestHandler<GetUserFilesQuery, PagedResult<CloudFileDto>>
{
    public async Task<PagedResult<CloudFileDto>> Handle(GetUserFilesQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (userId is null)
            throw new NotFoundException("Пользователь не найден");

        var user = await userRepository.GetByIdAsync(userId.Value);
        var userDisplayName = user!.DisplayName;

        var startQueryable = cloudFileRepository.Query().Where(f => f.IsDeleted == request.DeletedFiles);
        var pagedIntermediateResult = await cloudFileRepository
            .ToPagedIntermediateResultAsync(request.PageNumber, request.PageSize, startQueryable, cancellationToken);

        var queryable = pagedIntermediateResult.Queryable;
        var dtoQuery = queryable
            .Select(f => new CloudFileDto
            {
                Id = f.Id,
                UserDisplayName = userDisplayName,
                CloudFolderId = f.CloudFolderId,
                PublicUrl = f.PublicUrl == null ? null : f.PublicUrl.Value,
                Key = f.Key,
                Size = f.Size,
                Name = f.Name,
                Extension = f.Extension,
                DownloadCount = f.DownloadCount,
                CreatedAt = f.CreatedAt,
                ModifiedAt = f.ModifiedAt
            })
            .ToList();
        
        var pagedResult = new PagedResult<CloudFileDto>(
            dtoQuery, 
            pagedIntermediateResult.TotalCount,
            pagedIntermediateResult.PageNumber,
            pagedIntermediateResult.PageSize);

        return pagedResult;
    }
}