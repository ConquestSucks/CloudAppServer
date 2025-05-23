using System.Collections.Concurrent;
using CloudAppServer.Application.Interfaces;

namespace CloudAppServer.Infrastructure.Services;

public class AuthorizationService : IAuthorizationService
{
    private readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> _sessions = new();
    
    public bool TryCreateAuthorizationSession(string username)
    {
        if (_sessions.ContainsKey(username))
            return false;

        _sessions[username] = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        
        return true;
    }

    public async Task<bool> AuthorizeUser(string username, TimeSpan timeout)
    {
        if (!_sessions.TryGetValue(username, out var tcs))
            return false;
        
        var task = tcs.Task;
        var delay = Task.Delay(timeout);
        var completed = await Task.WhenAny(task, delay);
        
        _sessions.TryRemove(username, out _);
        
        return completed == task && task.Result;
    }

    public void ApproveUserAuthorization(string username)
    {
        if (_sessions.TryGetValue(username, out var tcs))
            tcs.TrySetResult(true);
    }

    public void DenyUserAuthorization(string username)
    {
        if (_sessions.TryGetValue(username, out var tcs))
            tcs.TrySetResult(false);
    }
}