using CloudAppServer.Infrastructure.SignalR;
using CloudAppServer.SharedKernel.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace CloudAppServer.Infrastructure.Notifications;

public class SignalRFileNotifier(IHubContext<FileUploadHub> hubContext) : IFileUploadNotifier
{
    public Task NotifyProgressAsync(string connectionId, int percent)
    {
        return hubContext.Clients.Client(connectionId).SendAsync("CurrentFileProgress", percent);
    }
}