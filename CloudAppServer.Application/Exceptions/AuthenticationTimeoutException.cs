using System.Net;

namespace CloudAppServer.Application.Exceptions;

public class AuthenticationTimeoutException(string message) : HttpException(HttpStatusCode.RequestTimeout, message);