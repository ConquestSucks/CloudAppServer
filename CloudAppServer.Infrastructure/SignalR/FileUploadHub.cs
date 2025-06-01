using Microsoft.AspNetCore.SignalR;

namespace CloudAppServer.Infrastructure.SignalR;

public class FileUploadHub : Hub
{
    public Task<string> GetConnectionId() => Task.FromResult(Context.ConnectionId);
}