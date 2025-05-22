namespace CloudAppServer.ConfigModels;

public class S3Config
{
    public string ServiceUrl { get; set; } = null!;
    public string BucketName { get; set; } = null!;
    public string Region { get; set; } = null!;
    public string AccessKey { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
}