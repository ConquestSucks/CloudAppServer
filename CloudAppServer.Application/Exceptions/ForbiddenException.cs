using System.Net;

namespace CloudAppServer.Application.Exceptions;

public class ForbiddenException(string message) : HttpException(HttpStatusCode.Forbidden, message);