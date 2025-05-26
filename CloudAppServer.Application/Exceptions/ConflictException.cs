using System.Net;

namespace CloudAppServer.Application.Exceptions;

public class ConflictException(string message) : HttpException(HttpStatusCode.Conflict, message);