namespace CloudAppServer.Domain.ValueObjects;

public sealed class PublicUrl
{
    public string Value { get; private set; }
    
    private PublicUrl(string value)
    {
        Value = value;
    }
    
    public static PublicUrl Create(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Url cannot be empty.", nameof(url));

        return new PublicUrl(url);
    }
    
    public override int GetHashCode() => Value.GetHashCode(StringComparison.InvariantCulture);
    public override string ToString() => Value;
}