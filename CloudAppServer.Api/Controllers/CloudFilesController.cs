using CloudAppServer.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudAppServer.Controllers;

[ApiController]
[Route("/api/v1/files/")]
[Authorize]
public class CloudFilesController(IS3Service s3Service) : ControllerBase
{
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file.Length == 0)
            return BadRequest("File is null or empty");

        await using var stream = file.OpenReadStream();
        await s3Service.UploadFileAsync(file.FileName, stream);

        return Ok();
    }
}