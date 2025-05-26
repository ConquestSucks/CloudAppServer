using System.Net;

namespace CloudAppServer.Application.Exceptions;

public class InternalServerErrorException(string message) : HttpException(HttpStatusCode.InternalServerError, message)
{
    
}