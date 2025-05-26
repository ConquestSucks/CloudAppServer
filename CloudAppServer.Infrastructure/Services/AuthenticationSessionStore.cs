using System.Collections.Concurrent;
using CloudAppServer.Application.Authentication.Interfaces;
using CloudAppServer.Application.Authentication.Models;

namespace CloudAppServer.Infrastructure.Services;

public class AuthenticationSessionStore : IAuthenticationSessionStore
{
    private readonly ConcurrentDictionary<string, AuthenticationSession> _sessions = new();
    
    public bool TryCreateAuthorizationSession(string username)
    {
        if (_sessions.ContainsKey(username))
            return false;

        _sessions[username] = new AuthenticationSession
        {
            TaskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously)
        };
        
        return true;
    }

    public async Task<bool> WaitForUserResponse(string username, TimeSpan timeout)
    {
        if (!_sessions.TryGetValue(username, out var session))
            return false;
        
        var task = session.TaskCompletionSource.Task;
        var delay = Task.Delay(timeout);
        var completed = await Task.WhenAny(task, delay);
        
        _sessions.TryRemove(username, out _);
        
        return completed == task && task.Result;
    }

    public void ApproveUserAuthorization(string username)
    {
        if (_sessions.TryGetValue(username, out var session))
            session.TaskCompletionSource.TrySetResult(true);
    }

    public void DenyUserAuthorization(string username)
    {
        if (_sessions.TryGetValue(username, out var session))
            session.TaskCompletionSource.TrySetResult(false);
    }
}