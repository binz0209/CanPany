namespace CanPany.Domain.Entities;

public class BackgroundJob
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string JobType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int RetryCount { get; set; } = 0;
    public int MaxRetries { get; set; } = 3;
    public string? ErrorMessage { get; set; }
}

