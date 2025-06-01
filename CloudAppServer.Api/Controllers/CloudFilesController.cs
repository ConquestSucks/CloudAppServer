using CloudAppServer.Application.DTOs;
using CloudAppServer.Application.Features.CloudFiles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace CloudAppServer.Controllers;

[ApiController]
[Route("/api/v1/files/")]
[Authorize]
public class CloudFilesController(IMediator mediator) : ControllerBase
{
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(
        IFormFile file, 
        [FromQuery] Guid? cloudFolderId,
        [FromQuery] string connectionId,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0)
            return BadRequest("Файл пустой");

        await using var stream = file.OpenReadStream();
        await mediator.Send(new UploadFileCommand
        {
            Key = file.FileName,
            Stream = stream,
            CloudFolderId = cloudFolderId,
            ConnectionId = connectionId
        }, cancellationToken);

        return Ok();
    }
    
    [HttpGet("download/{key}")]
    public async Task<IActionResult> Download(string key)
    {
        var (stream, contentType, displayName) = await mediator.Send(new DownloadFileQuery
        {
            Key = key
        });
        
        return File(stream, contentType, displayName);
    }

    [HttpGet("getUserFiles")]
    public async Task<IReadOnlyList<CloudFileDto>> GetUserFiles(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10,
        [FromQuery] bool deletedFiles = false)
    {
        var pagedResult = await mediator.Send(new GetUserFilesQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            DeletedFiles = deletedFiles
        });
        
        HttpContext.Response.Headers.Append("X-Total-Count", new StringValues(pagedResult.TotalCount.ToString()));
        HttpContext.Response.Headers.Append("X-Total-Pages", new StringValues(pagedResult.TotalPages.ToString()));
        HttpContext.Response.Headers.Append("X-Page-Size", new StringValues(pagedResult.PageSize.ToString()));
        HttpContext.Response.Headers.Append("X-Page-Number", new StringValues(pagedResult.PageNumber.ToString()));
        
        return pagedResult.Items;
    }

    [HttpGet("getUserFile/{key}")]
    public async Task<CloudFileDto> GetUserFile(string key, [FromQuery] bool deletedFile = false)
    {
        return await mediator.Send(new GetUserFileQuery
        {
            Key = key,
            DeletedFile = deletedFile
        });
    }

    [HttpDelete("deleteFile/{key}")]
    public async Task<IActionResult> DeleteFile(string key)
    {
        await mediator.Send(new DeleteFileCommand
        {
            Key = key
        });

        return Ok();
    }

    [HttpDelete("deleteFileWithoutRemove/{key}")]
    public async Task<IActionResult> DeleteFileWithoutRemove(string key)
    {
        await mediator.Send(new DeleteFileWithoutRemoveCommand
        {
            Key = key
        });

        return Ok();
    }

    [HttpPut("restoreFile")]
    public async Task<CloudFileDto> RestoreFile(RestoreFileCommand command)
    {
        return await mediator.Send(command);
    }
}