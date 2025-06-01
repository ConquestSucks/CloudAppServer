using System.Net;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using CloudAppServer.Application.Exceptions;
using CloudAppServer.Application.Interfaces;
using CloudAppServer.ConfigModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CloudAppServer.Infrastructure.Services
{
    public class S3Service : IS3Service
    {
        private readonly AmazonS3Client _client;
        private readonly string _bucket;
        private const long PartSize = 50 * 1024 * 1024;

        private ILogger<S3Service> _logger;

        public S3Service(IOptions<S3Config> opts, ILogger<S3Service> logger)
        {
            var s3Config = opts.Value;
            var credentials = new BasicAWSCredentials(s3Config.AccessKey, s3Config.SecretKey);
            var config = new AmazonS3Config
            {
                ServiceURL = s3Config.ServiceUrl,
                ForcePathStyle = true
            };
            
            _bucket = s3Config.BucketName;
            _client = new AmazonS3Client(credentials, config);

            _logger = logger;
        }

        public async Task UploadFileAsync(string key, Stream data, Action<int> progressCallback, CancellationToken cancellationToken = default)
        {
            if (data.CanSeek)
            {
                var totalSize = data.Length;
                if (totalSize <= PartSize)
                {
                    await UploadSinglePartAsync(key, data, progressCallback, cancellationToken);
                    
                    return;
                }
                
                await UploadMultipartAsync(key, data, totalSize, progressCallback, cancellationToken);
            }
            else
            {
                await UploadSinglePartAsync(key, data, progressCallback, cancellationToken);
            }
        }

        private async Task UploadSinglePartAsync(string key, Stream data, Action<int> progressCallback, CancellationToken cancellationToken)
        {
            var transferUtility = new TransferUtility(_client);
            var request = new TransferUtilityUploadRequest
            {
                InputStream = data,
                BucketName = _bucket,
                Key = key
            };
            
            request.UploadProgressEvent += (_, args) =>
            {
                if (args.TotalBytes <= 0) 
                    return;
                
                var percent = (int)(args.TransferredBytes * 100L / args.TotalBytes);
                progressCallback(percent);
            };
            
            await transferUtility.UploadAsync(request, cancellationToken);
            
            progressCallback.Invoke(100);
        }

        private async Task UploadMultipartAsync(string key, Stream data, long totalSize, Action<int> progressCallback, CancellationToken cancellationToken)
        {
            data.Position = 0;
            var uploadId = await InitiateMultipartAsync(key, cancellationToken);
            
            try
            {
                var partETags = await UploadAllPartsAsync(key, data, totalSize, uploadId, progressCallback, cancellationToken);
                if (partETags.Count == 0)
                {
                    await AbortMultipartAsync(key, uploadId);
                    
                    throw new InternalServerErrorException("Не удалось загрузить файл");
                }
                
                await CompleteMultipartAsync(key, uploadId, partETags, cancellationToken);
            }
            catch
            {
                await AbortMultipartAsync(key, uploadId);
                
                throw new InternalServerErrorException("Не удалось загрузить файл");
            }
        }

        private async Task<string> InitiateMultipartAsync(string key, CancellationToken cancellationToken)
        {
            var initRequest = new InitiateMultipartUploadRequest
            {
                BucketName = _bucket,
                Key = key
            };
            var initResponse = await _client.InitiateMultipartUploadAsync(initRequest, cancellationToken);
            
            return initResponse.UploadId;
        }

        private async Task<List<PartETag>> UploadAllPartsAsync(string key, Stream data, long totalSize, string uploadId, Action<int> progressCallback, CancellationToken cancellationToken)
        {
            var partETags = new List<PartETag>();
            long filePosition = 0;
            var partNumber = 1;
            
            while (filePosition < totalSize)
            {
                var currentPartSize = Math.Min(PartSize, totalSize - filePosition);
                var buffer = new byte[currentPartSize];
                
                data.Seek(filePosition, SeekOrigin.Begin);
                var bytesRead = await data.ReadAsync(buffer.AsMemory(0, (int)currentPartSize), cancellationToken);
                if (bytesRead == 0)
                    break;
                
                var eTag = await UploadSinglePartPieceAsync(key, uploadId, partNumber, buffer, bytesRead, cancellationToken);
                
                partETags.Add(new PartETag(partNumber, eTag));
                
                filePosition += bytesRead;
                var percent = (int)Math.Min(100, (filePosition * 100L) / totalSize);
                
                progressCallback?.Invoke(percent);
                
                partNumber++;
            }
            
            return partETags;
        }

        private async Task<string> UploadSinglePartPieceAsync(string key, string uploadId, int partNumber, byte[] buffer, int bytesRead, CancellationToken cancellationToken)
        {
            using var ms = new MemoryStream(buffer, 0, bytesRead);
            
            var uploadPartRequest = new UploadPartRequest
            {
                BucketName = _bucket,
                Key = key,
                UploadId = uploadId,
                PartNumber = partNumber,
                PartSize = bytesRead,
                InputStream = ms
            };
            var uploadPartResponse = await _client.UploadPartAsync(uploadPartRequest, cancellationToken);
            
            return uploadPartResponse.ETag;
        }

        private async Task CompleteMultipartAsync(string key, string uploadId, List<PartETag> partETags, CancellationToken cancellationToken)
        {
            var completeRequest = new CompleteMultipartUploadRequest
            {
                BucketName = _bucket,
                Key = key,
                UploadId = uploadId,
                PartETags = partETags
            };
            
            await _client.CompleteMultipartUploadAsync(completeRequest, cancellationToken);
        }

        private async Task AbortMultipartAsync(string key, string uploadId)
        {
            var abortRequest = new AbortMultipartUploadRequest
            {
                BucketName = _bucket,
                Key = key,
                UploadId = uploadId
            };
            
            var abortResponse = await _client.AbortMultipartUploadAsync(abortRequest, CancellationToken.None);
            if (((int)abortResponse.HttpStatusCode).ToString()[0] != '2')
                _logger.LogCritical("AbortMultipartUpload {UploadId} {Key} is not successful", uploadId, key);
        }

        public async Task<Stream> DownloadFileAsync(string key)
        {
            var resp = await _client.GetObjectAsync(_bucket, key);
            if (resp == null || resp.HttpStatusCode != HttpStatusCode.OK)
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
}