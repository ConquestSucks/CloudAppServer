using CloudApp.SharedKernel.Interfaces;

namespace CloudApp.Infrastructure;

public class UtcDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}