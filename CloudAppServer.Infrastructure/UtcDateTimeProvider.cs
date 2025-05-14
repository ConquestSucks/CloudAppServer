using CloudAppServer.SharedKernel.Interfaces;

namespace CloudAppServer.Infrastructure;

public class UtcDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}