using System.Net;

namespace CloudAppServer.Application.Exceptions;

public abstract class HttpException(HttpStatusCode statusCode, string message) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}