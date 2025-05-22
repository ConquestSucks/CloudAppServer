namespace CloudAppServer.Application.Interfaces;

public interface IS3Service
{
    Task UploadFileAsync(string key, Stream data);
    Task<Stream> DownloadFileAsync(string key);
    Task DeleteFileAsync(string key);
    Task<List<string>> ListFilesAsync(string? prefix = null);
}