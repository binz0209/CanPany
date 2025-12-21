using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace CanPany.Infrastructure.Services;

public class RedisBackgroundJobService : IBackgroundJobService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisBackgroundJobService> _logger;
    private readonly string _queueKey = "background:jobs:queue";
    private readonly string _processingKey = "background:jobs:processing";
    private readonly string _completedKey = "background:jobs:completed";
    private readonly string _failedKey = "background:jobs:failed";

    public RedisBackgroundJobService(
        IConnectionMultiplexer redis,
        ILogger<RedisBackgroundJobService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<string> EnqueueJobAsync<T>(string jobType, T payload)
    {
        var db = _redis.GetDatabase();
        var job = new BackgroundJob
        {
            JobType = jobType,
            Payload = JsonSerializer.Serialize(payload)
        };

        var jobJson = JsonSerializer.Serialize(job);
        await db.ListLeftPushAsync(_queueKey, jobJson);
        
        _logger.LogInformation("Job {JobId} of type {JobType} enqueued", job.Id, jobType);
        return job.Id;
    }

    public async Task<BackgroundJob?> DequeueJobAsync()
    {
        var db = _redis.GetDatabase();
        var jobJson = await db.ListRightPopLeftPushAsync(_queueKey, _processingKey);
        
        if (jobJson.IsNullOrEmpty)
            return null;

        var job = JsonSerializer.Deserialize<BackgroundJob>(jobJson!);
        return job;
    }

    public async Task<bool> CompleteJobAsync(string jobId)
    {
        var db = _redis.GetDatabase();
        var processingJobs = await db.ListRangeAsync(_processingKey);
        
        foreach (var jobJson in processingJobs)
        {
            var job = JsonSerializer.Deserialize<BackgroundJob>(jobJson!);
            if (job?.Id == jobId)
            {
                await db.ListRemoveAsync(_processingKey, jobJson);
                await db.ListLeftPushAsync(_completedKey, jobJson);
                _logger.LogInformation("Job {JobId} completed", jobId);
                return true;
            }
        }
        return false;
    }

    public async Task<bool> FailJobAsync(string jobId, string error)
    {
        var db = _redis.GetDatabase();
        var processingJobs = await db.ListRangeAsync(_processingKey);
        
        foreach (var jobJson in processingJobs)
        {
            var job = JsonSerializer.Deserialize<BackgroundJob>(jobJson!);
            if (job?.Id == jobId)
            {
                await db.ListRemoveAsync(_processingKey, jobJson);
                job.RetryCount++;
                job.ErrorMessage = error;
                
                if (job.RetryCount < job.MaxRetries)
                {
                    // Retry: đưa lại vào queue
                    var retryJobJson = JsonSerializer.Serialize(job);
                    await db.ListLeftPushAsync(_queueKey, retryJobJson);
                    _logger.LogWarning(
                        "Job {JobId} failed, retrying ({RetryCount}/{MaxRetries}): {Error}",
                        jobId, job.RetryCount, job.MaxRetries, error);
                }
                else
                {
                    // Max retries reached, move to failed
                    await db.ListLeftPushAsync(_failedKey, jobJson);
                    _logger.LogError("Job {JobId} failed permanently: {Error}", jobId, error);
                }
                return true;
            }
        }
        return false;
    }
}

