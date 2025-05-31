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
    public async Task<IActionResult> UploadFile(IFormFile file, [FromQuery] Guid? cloudFolderId)
    {
        if (file.Length == 0)
            return BadRequest("Файл пустой");

        await using var stream = file.OpenReadStream();
        await mediator.Send(new UploadFileCommand
        {
            Key = file.FileName,
            Stream = stream,
            CloudFolderId = cloudFolderId
        });

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
        [FromQuery] int pageSize = 10)
    {
        var pagedResult = await mediator.Send(new GetUserFilesQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        });
        
        HttpContext.Response.Headers.Append("X-Total-Count", new StringValues(pagedResult.TotalCount.ToString()));
        HttpContext.Response.Headers.Append("X-Total-Pages", new StringValues(pagedResult.TotalPages.ToString()));
        HttpContext.Response.Headers.Append("X-Page-Size", new StringValues(pagedResult.PageSize.ToString()));
        HttpContext.Response.Headers.Append("X-Page-Number", new StringValues(pagedResult.PageNumber.ToString()));
        
        return pagedResult.Items;
    }
}