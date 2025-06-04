using BusinessObjects.Enums;
using Helpers.DTOs.FileHandler;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implements
{
    public class FileHandlerService : IFileHandlerService
    {
        private readonly IMinioService _minioService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileHandlerService> _logger;
        private readonly string[] bucketNames = Enum.GetNames(typeof(BucketNames))
                         .Select(name => name.ToLower())
                         .ToArray();
        private readonly string[] _allowedVideoTypes;
        private readonly string[] _allowedImageTypes;
        public FileHandlerService(IMinioService minioService, ILogger<FileHandlerService> logger, IConfiguration configuration)
        {
            _minioService = minioService;
            _logger = logger;
            _allowedVideoTypes = new[] { "video/mp4", "video/x-msvideo", "video/quicktime", "video/x-ms-wmv", "video/webm" };
            _allowedImageTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/bmp", "image/webp" , "image/svg+xml" };
            _configuration = configuration;
        }
        public async Task<SingleUploadResult> UploadSingleFile(IFormFile file, string bucketName, string courseName, string fileType, int index)
        {
            var result = new SingleUploadResult();

            try
            {
                // Generate unique filename
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var sanitizedCourseName = SanitizeFileName(courseName);
                var fileExtension = Path.GetExtension(file.FileName);
                //var storedFileName = $"{sanitizedCourseName}/{fileType}/{timestamp}_{index:D2}_{Guid.NewGuid():N}{fileExtension}";
                var storedFileName = $"{timestamp}_{index:D2}_{Guid.NewGuid():N}{fileExtension}";

                // Upload to Minio
                using var stream = file.OpenReadStream();
                var uploadSuccess = await _minioService.PutObjectAsync(
                    bucketName,
                    storedFileName,
                    stream,
                    file.Length,
                    file.ContentType
                );

                if (uploadSuccess)
                {
                    // Generate presigned URL (optional, 24 hours expiry)
                    string? presignedUrl = null;
                    try
                    {
                        var host = _configuration["MinIO:Endpoint"];
                        //var schema = _configuration["Minio:Schema"];
                        presignedUrl = $"http://{host}/{bucketName}/{storedFileName}";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to generate presigned URL for {FileName}", storedFileName);
                    }

                    result.Success = true;
                    result.FileInfo = new FileUploadInfo
                    {
                        OriginalFileName = file.FileName,
                        StoredFileName = storedFileName,
                        BucketName = bucketName,
                        FileSize = file.Length,
                        ContentType = file.ContentType ?? "application/octet-stream",
                        UploadedAt = DateTime.UtcNow,
                        PresignedUrl = presignedUrl,
                        UploadField = fileType
                    };

                    _logger.LogInformation("Successfully uploaded {FileName} as {StoredFileName}", file.FileName, storedFileName);
                }
                else
                {
                    result.Errors.Add($"Failed to upload {file.FileName}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file {FileName}", file.FileName);
                result.Errors.Add($"Error uploading {file.FileName}: {ex.Message}");
            }

            return result;
        }

        private static string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());
            return sanitized.Replace(" ", "_").ToLowerInvariant();
        }

        public async Task<UploadResult> UploadFiles(List<IFormFile> files, string courseName, string fileType)
        {
            var result = new UploadResult();
            var semaphore = new SemaphoreSlim(3, 3); // Limit concurrent uploads

            var tasks = files.Select(async (file, index) =>
            {
                await semaphore.WaitAsync();
                try
                {
                    string bucketName = string.Empty;
                    if (_allowedVideoTypes.Contains(file.ContentType?.ToLower()))
                    {
                        bucketName = "video";
                    } else if (_allowedImageTypes.Contains(file.ContentType?.ToLower()))
                    {
                        bucketName = "image";
                    } else
                    {
                        bucketName = "document";
                    }

                    return await UploadSingleFile(file, bucketName, courseName, fileType, index);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            var results = await Task.WhenAll(tasks);

            foreach (var singleResult in results)
            {
                if (singleResult.Success && singleResult.FileInfo != null)
                {
                    result.SuccessfulUploads.Add(singleResult.FileInfo);
                }
                else
                {
                    result.Errors.AddRange(singleResult.Errors);
                }
            }

            return result;
        }
    }
}
