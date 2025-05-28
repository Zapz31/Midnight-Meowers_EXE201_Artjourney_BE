using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Enums;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;

namespace Helpers.BackgroundService
{
    public class MinioBucketInitializer : IHostedService
    {
        private readonly IMinioClient _minioClient;
        private readonly ILogger<MinioBucketInitializer> _logger;

        public MinioBucketInitializer(IMinioClient minioClient, ILogger<MinioBucketInitializer> logger)
        {
            _minioClient = minioClient;
            _logger = logger;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var bucket in Enum.GetNames(typeof(BucketNames)))
            {
                try
                {
                    var bucketName = bucket.ToLower(); // MinIO bucket names should be lowercase
                    var existsArgs = new BucketExistsArgs().WithBucket(bucketName);
                    bool exists = await _minioClient.BucketExistsAsync(existsArgs, cancellationToken).ConfigureAwait(false);

                    if (!exists)
                    {
                        var makeBucketArgs = new MakeBucketArgs().WithBucket(bucketName);
                        await _minioClient.MakeBucketAsync(makeBucketArgs, cancellationToken).ConfigureAwait(false);
                        _logger.LogInformation("Bucket '{BucketName}' created successfully.", bucketName);
                    }
                    else
                    {
                        _logger.LogInformation("Bucket '{BucketName}' already exists.", bucketName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking or creating bucket '{BucketName}'.", bucket);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    }
}
