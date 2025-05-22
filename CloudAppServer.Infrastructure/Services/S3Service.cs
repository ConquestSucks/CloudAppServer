using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using CloudAppServer.Application.Interfaces;
using CloudAppServer.ConfigModels;
using Microsoft.Extensions.Options;

namespace CloudAppServer.Infrastructure.Services;

public class S3Service : IS3Service
{
    private readonly AmazonS3Client _client;
    private readonly string _bucket;

    public S3Service(IOptions<S3Config> opts)
    {
        var s3Config = opts.Value;
        var credentials  = new BasicAWSCredentials(s3Config.AccessKey, s3Config.SecretKey);
        var config = new AmazonS3Config
        {
            ServiceURL = s3Config.ServiceUrl,
            //RegionEndpoint = RegionEndpoint.GetBySystemName(s3Config.Region),
            ForcePathStyle = true
        };
        
        _bucket = s3Config.BucketName;
        _client = new AmazonS3Client(credentials, config);
    }

    public async Task UploadFileAsync(string key, Stream data)
    {
        var req = new PutObjectRequest
        {
            BucketName = _bucket,
            Key = key,
            InputStream = data
        };
        
        await _client.PutObjectAsync(req);
    }

    public async Task<Stream> DownloadFileAsync(string key)
    {
        var resp = await _client.GetObjectAsync(_bucket, key);
        var ms = new MemoryStream();
        
        await resp.ResponseStream.CopyToAsync(ms);
        
        ms.Position = 0;
        
        return ms;
    }

    public Task DeleteFileAsync(string key) => _client.DeleteObjectAsync(_bucket, key);

    public async Task<List<string>> ListFilesAsync(string? prefix = null)
    {
        var req = new ListObjectsV2Request
        {
            BucketName = _bucket,
            Prefix = prefix ?? string.Empty
        };
        
        var resp = await _client.ListObjectsV2Async(req);
        
        return resp.S3Objects.Select(o => o.Key).ToList();
    }
}