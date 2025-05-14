namespace CloudAppServer.SharedKernel.Interfaces;

public interface IDateTimeProvider
{
    public DateTime UtcNow { get; }
}