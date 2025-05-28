using Minio.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IMinioService
    {
        Task<bool> BucketExistsAsync(string bucketName);
        Task CreateBucketAsync(string bucketName);
        Task<bool> PutObjectAsync(string bucketName, string objectName, string filePath, string? contentType = null);
        Task<bool> PutObjectAsync(string bucketName, string objectName, Stream stream, long size, string? contentType = null);
        Task<Stream> GetObjectAsync(string bucketName, string objectName);
        Task<bool> GetObjectAsync(string bucketName, string objectName, string filePath);
        Task<bool> RemoveObjectAsync(string bucketName, string objectName);
        Task<IEnumerable<string>> ListObjectsAsync(string bucketName, string? prefix = null);
        Task<ObjectStat> GetObjectInfoAsync(string bucketName, string objectName);
        Task<string> GetPresignedObjectUrlAsync(string bucketName, string objectName, int expiry = 3600);
    }
}
