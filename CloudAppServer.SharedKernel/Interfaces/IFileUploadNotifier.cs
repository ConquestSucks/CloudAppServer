namespace CloudAppServer.SharedKernel.Interfaces;

public interface IFileUploadNotifier
{
    Task NotifyProgressAsync(string connectionId, int percent);
}