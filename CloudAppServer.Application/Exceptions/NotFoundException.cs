using System.Net;

namespace CloudAppServer.Application.Exceptions;

public class NotFoundException(string message) : HttpException(HttpStatusCode.NotFound, message);