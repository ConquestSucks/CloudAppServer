namespace CloudAppServer.Application.Authentication.Models;

public class AuthenticationSession
{
    public required TaskCompletionSource<bool> TaskCompletionSource { get; set; }
}