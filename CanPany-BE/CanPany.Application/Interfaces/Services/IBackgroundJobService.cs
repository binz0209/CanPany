namespace CanPany.Application.Interfaces.Services;

public interface IBackgroundJobService
{
    Task<string> EnqueueJobAsync<T>(string jobType, T payload);
    Task<Domain.Entities.BackgroundJob?> DequeueJobAsync();
    Task<bool> CompleteJobAsync(string jobId);
    Task<bool> FailJobAsync(string jobId, string error);
}

