using Microsoft.Extensions.Logging;
using Minio;
using Minio.ApiEndpoints;
using Minio.DataModel;
using Minio.DataModel.Args;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implements
{
    public class MinioService : IMinioService
    {
        private readonly IMinioClient _minioClient;
        private readonly ILogger<MinioService> _logger;
        public MinioService(IMinioClient minioClient, ILogger<MinioService> logger)
        {
            _minioClient = minioClient;
            _logger = logger;
        }

        /// <summary>
        /// Kiểm tra xem bucket có tồn tại hay không
        /// </summary>
        public async Task<bool> BucketExistsAsync(string bucketName)
        {
            try
            {
                var args = new BucketExistsArgs().WithBucket(bucketName);
                return await _minioClient.BucketExistsAsync(args);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking if bucket {BucketName} exists", bucketName);
                return false;
            }
        }

        /// <summary>
        /// Tạo bucket mới
        /// </summary>
        public async Task CreateBucketAsync(string bucketName)
        {
            try
            {
                var bucketExists = await BucketExistsAsync(bucketName);
                if (!bucketExists)
                {
                    var args = new MakeBucketArgs().WithBucket(bucketName);
                    await _minioClient.MakeBucketAsync(args);
                    _logger?.LogInformation("Created bucket: {BucketName}", bucketName);
                }
                else
                {
                    _logger?.LogInformation("Bucket {BucketName} already exists", bucketName);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating bucket {BucketName}", bucketName);
                throw;
            }
        }

        /// <summary>
        /// Download object và trả về Stream
        /// </summary>
        public async Task<Stream> GetObjectAsync(string bucketName, string objectName)
        {
            try
            {
                var memoryStream = new MemoryStream();
                var args = new GetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithCallbackStream(stream => stream.CopyTo(memoryStream));

                await _minioClient.GetObjectAsync(args);
                memoryStream.Position = 0;
                _logger?.LogInformation("Successfully downloaded {ObjectName} from bucket {BucketName}", objectName, bucketName);
                return memoryStream;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error downloading object {ObjectName} from bucket {BucketName}", objectName, bucketName);
                throw;
            }
        }

        /// <summary>
        /// Download object và lưu vào file
        /// </summary>
        public async Task<bool> GetObjectAsync(string bucketName, string objectName, string filePath)
        {
            try
            {
                // Tạo thư mục nếu chưa tồn tại
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var args = new GetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithFile(filePath);

                await _minioClient.GetObjectAsync(args);
                _logger?.LogInformation("Successfully downloaded {ObjectName} to {FilePath}", objectName, filePath);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error downloading object {ObjectName} to {FilePath}", objectName, filePath);
                return false;
            }
        }

        /// <summary>
        /// Lấy thông tin metadata của object
        /// </summary>
        public async Task<ObjectStat> GetObjectInfoAsync(string bucketName, string objectName)
        {
            try
            {
                var args = new StatObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName);

                var objectStat = await _minioClient.StatObjectAsync(args);
                _logger?.LogInformation("Retrieved info for {ObjectName} from bucket {BucketName}", objectName, bucketName);
                return objectStat;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting info for object {ObjectName} from bucket {BucketName}", objectName, bucketName);
                throw;
            }
        }

        public async Task<string> GetPresignedObjectUrlAsync(string bucketName, string objectName, int expiry)
        {
            try
            {
                var args = new PresignedGetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithExpiry(expiry);

                var url = await _minioClient.PresignedGetObjectAsync(args);
                _logger?.LogInformation("Generated presigned URL for {ObjectName} from bucket {BucketName}", objectName, bucketName);
                return url;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating presigned URL for {ObjectName} from bucket {BucketName}", objectName, bucketName);
                throw;
            }
        }

        /// <summary>
        /// Liệt kê các object trong bucket
        /// </summary>
        public async Task<IEnumerable<string>> ListObjectsAsync(string bucketName, string? prefix = null)
        {
            try
            {
                var objects = new List<string>();
                var args = new ListObjectsArgs()
                    .WithBucket(bucketName)
                    .WithPrefix(prefix)
                    .WithRecursive(true);

                await foreach (var item in _minioClient.ListObjectsEnumAsync(args).ConfigureAwait(false))
                {
                    objects.Add(item.Key);
                }

                _logger?.LogInformation("Listed {Count} objects from bucket {BucketName}", objects.Count, bucketName);
                return objects;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error listing objects from bucket {BucketName}", bucketName);
                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// Upload file từ đường dẫn local
        /// </summary>
        public async Task<bool> PutObjectAsync(string bucketName, string objectName, string filePath, string? contentType = null)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"File not found: {filePath}");
                }

                // Tự động detect content type nếu không được cung cấp
                contentType ??= GetContentType(filePath);

                var args = new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithFileName(filePath)
                    .WithContentType(contentType);

                await _minioClient.PutObjectAsync(args);
                _logger?.LogInformation("Successfully uploaded {ObjectName} to bucket {BucketName}", objectName, bucketName);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error uploading object {ObjectName} to bucket {BucketName}", objectName, bucketName);
                return false;
            }
        }

        /// <summary>
        /// Upload từ Stream
        /// </summary>
        public async Task<bool> PutObjectAsync(string bucketName, string objectName, Stream stream, long size, string? contentType = null)
        {
            try
            {
                contentType ??= "application/octet-stream";

                var args = new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithStreamData(stream)
                    .WithObjectSize(size)
                    .WithContentType(contentType);

                await _minioClient.PutObjectAsync(args);
                _logger?.LogInformation("Successfully uploaded {ObjectName} to bucket {BucketName}", objectName, bucketName);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error uploading object {ObjectName} to bucket {BucketName}", objectName, bucketName);
                return false;
            }
        }

        /// <summary>
        /// Xóa object
        /// </summary>
        public async Task<bool> RemoveObjectAsync(string bucketName, string objectName)
        {
            try
            {
                var args = new RemoveObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName);

                await _minioClient.RemoveObjectAsync(args);
                _logger?.LogInformation("Successfully removed {ObjectName} from bucket {BucketName}", objectName, bucketName);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error removing object {ObjectName} from bucket {BucketName}", objectName, bucketName);
                return false;
            }
        }

        /// <summary>
        /// Helper method để tự động detect content type từ file extension
        /// </summary>
        private static string GetContentType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".pdf" => "application/pdf",
                ".txt" => "text/plain",
                ".json" => "application/json",
                ".xml" => "application/xml",
                ".zip" => "application/zip",
                ".mp4" => "video/mp4",
                ".mp3" => "audio/mpeg",
                _ => "application/octet-stream"
            };
        }
    }
}
