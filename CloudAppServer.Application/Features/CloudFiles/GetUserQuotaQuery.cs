using CloudAppServer.Application.DTOs;
using CloudAppServer.Application.Exceptions;
using CloudAppServer.Application.Interfaces;
using CloudAppServer.Domain.Interfaces;
using MediatR;

namespace CloudAppServer.Application.Features.CloudFiles;

public class GetUserQuotaQuery : IRequest<List<QuotaItemDto>>;

public class GetUserQuotaQueryHandler(
    ICloudFileRepository cloudFileRepository,
    ICurrentUserService currentUserService) 
    : IRequestHandler<GetUserQuotaQuery, List<QuotaItemDto>>
{
    private readonly List<string> _imageExtensions =
    [
        "jpg", 
        "jpeg", 
        "png", 
        "gif",
        "bmp",
        "svg",
        "tiff",
        "webp"
    ];
    
    private readonly List<string> _videoExtensions =
    [
        "mp4", 
        "mkv", 
        "avi", 
        "mov",
        "wmv",
        "flv",
        "webm"
    ];
    
    private readonly List<string> _documentExtensions =
    [
        "pdf", 
        "doc", 
        "docx", 
        "txt",
        "xls",
        "xlsx",
        "ppt",
        "pptx",
        "odt",
        "rtf"
    ];
    
    private readonly List<string> _archiveExtensions =
    [
        "zip", 
        "rar", 
        "7z", 
        "tar",
        "gz",
        "bz2",
        "xz"
    ];
    
    public async Task<List<QuotaItemDto>> Handle(GetUserQuotaQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (userId is null)
            throw new NotFoundException("Пользователь не найден");

        var cloudFiles = await cloudFileRepository.GetUserCloudFilesByUserIdAsync(userId.Value);
        var grouped = cloudFiles
            .GroupBy(f =>
                f.Extension.ToLowerInvariant() switch
                {
                    var ext when _imageExtensions.Contains(ext) => "Изображения",
                    var ext when _videoExtensions.Contains(ext) => "Видео",
                    var ext when _documentExtensions.Contains(ext) => "Документы",
                    var ext when _archiveExtensions.Contains(ext) => "Архивы",
                    _ => "Прочее"
                }
            )
            .Select(g => new QuotaItemDto
            {
                FileType = g.Key,
                TotalSize = g.Sum(f => f.Size)
            })
            .ToList();
        
        var allCategories = new[] { "Изображения", "Видео", "Документы", "Архивы", "Прочее" };
        
        foreach (var cat in allCategories)
        {
            if (grouped.All(x => x.FileType != cat))
            {
                grouped.Add(new QuotaItemDto
                {
                    FileType = cat,
                    TotalSize = 0
                });
            }
        }
        
        return grouped
            .OrderBy(x => Array.IndexOf(allCategories, x.FileType))
            .ToList();
    }
}