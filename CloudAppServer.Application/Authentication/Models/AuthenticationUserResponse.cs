namespace CloudAppServer.Application.Authentication.Models;

public class AuthenticationUserResponse
{
    public bool IsTimeout { get; set; }
    
    public bool IsAuthenticationApproved { get; set; }
}