using System.Net;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using CloudAppServer.Application.Exceptions;
using CloudAppServer.Application.Interfaces;
using CloudAppServer.ConfigModels;
using Microsoft.Extensions.Options;

namespace CloudAppServer.Infrastructure.Services;

public class S3Service : IS3Service
{
    private readonly AmazonS3Client _client;
    private readonly string _bucket;
    
    private const long PartSize = 50 * 1024 * 1024; // 50 мегабайт

    public S3Service(IOptions<S3Config> opts)
    {
        var s3Config = opts.Value;
        var credentials  = new BasicAWSCredentials(s3Config.AccessKey, s3Config.SecretKey);
        var config = new AmazonS3Config
        {
            ServiceURL = s3Config.ServiceUrl,
            ForcePathStyle = true
        };
        
        _bucket = s3Config.BucketName;
        _client = new AmazonS3Client(credentials, config);
    }

    public async Task UploadFileAsync(string key, Stream data)
    {
        if (data.CanSeek)
        {
            var totalSize = data.Length;
            if (totalSize <= PartSize)
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = _bucket,
                    Key = key,
                    InputStream = data
                };
                await _client.PutObjectAsync(putRequest);
                return;
            }
            
            data.Position = 0;
            var initRequest = new InitiateMultipartUploadRequest
            {
                BucketName = _bucket,
                Key = key
            };
            var initResponse = await _client.InitiateMultipartUploadAsync(initRequest);
            var uploadId = initResponse.UploadId;

            try
            {
                var partETags = new List<PartETag>();
                long filePosition = 0;
                var partNumber = 1;
                
                while (filePosition < totalSize)
                {
                    var currentPartSize = Math.Min(PartSize, totalSize - filePosition);
                    var buffer = new byte[currentPartSize];

                    data.Seek(filePosition, SeekOrigin.Begin);
                    var bytesRead = await data.ReadAsync(buffer.AsMemory(0, (int)currentPartSize));
                    if (bytesRead == 0)
                        break;
                    
                    using (var memStream = new MemoryStream(buffer, 0, bytesRead))
                    {
                        var uploadRequest = new UploadPartRequest
                        {
                            BucketName = _bucket,
                            Key = key,
                            UploadId = uploadId,
                            PartNumber = partNumber,
                            PartSize = bytesRead,
                            InputStream = memStream
                        };

                        var uploadResponse = await _client.UploadPartAsync(uploadRequest);
                        partETags.Add(new PartETag(partNumber, uploadResponse.ETag));
                    }

                    filePosition += bytesRead;
                    partNumber++;
                }
                
                var completeRequest = new CompleteMultipartUploadRequest
                {
                    BucketName = _bucket,
                    Key = key,
                    UploadId = uploadId
                };
                completeRequest.AddPartETags(partETags);
                
                await _client.CompleteMultipartUploadAsync(completeRequest);
            }
            catch (Exception)
            {
                var abortRequest = new AbortMultipartUploadRequest
                {
                    BucketName = _bucket,
                    Key = key,
                    UploadId = uploadId
                };
                await _client.AbortMultipartUploadAsync(abortRequest);
                throw;
            }
        }
        else
        {
            var putRequest = new PutObjectRequest
            {
                BucketName = _bucket,
                Key = key,
                InputStream = data
            };
            await _client.PutObjectAsync(putRequest);
        }
    }

    public async Task<Stream> DownloadFileAsync(string key)
    {
        var resp = await _client.GetObjectAsync(_bucket, key);
        if (resp is null || resp.HttpStatusCode != HttpStatusCode.OK)
            throw new NotFoundException($"Файл {key} не найден");
        
        return resp.ResponseStream;
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