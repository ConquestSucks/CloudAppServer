using System.Net;

namespace CloudAppServer.Application.Exceptions;

public class NotEnoughDiskSpaceException() : HttpException(HttpStatusCode.Conflict, "Недостаточно места на диске");