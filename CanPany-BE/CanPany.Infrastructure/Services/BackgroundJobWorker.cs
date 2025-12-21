using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CanPany.Infrastructure.Services;

public class BackgroundJobWorker : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<BackgroundJobWorker> _logger;
    private readonly int _pollInterval;

    public BackgroundJobWorker(
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        ILogger<BackgroundJobWorker> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _pollInterval = configuration.GetValue<int>("BackgroundJobs:PollInterval", 1000);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var jobService = scope.ServiceProvider.GetRequiredService<IBackgroundJobService>();
                
                var job = await jobService.DequeueJobAsync();
                
                if (job != null)
                {
                    await ProcessJobAsync(job, scope.ServiceProvider, jobService);
                }
                else
                {
                    await Task.Delay(_pollInterval, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in background job worker");
                await Task.Delay(_pollInterval, stoppingToken);
            }
        }
    }

    private async Task ProcessJobAsync(BackgroundJob job, IServiceProvider serviceProvider, IBackgroundJobService jobService)
    {
        try
        {
            _logger.LogInformation("Processing job {JobId} of type {JobType}", job.Id, job.JobType);

            // Dispatch to appropriate handler
            var handler = GetJobHandler(serviceProvider, job.JobType);
            
            if (handler != null)
            {
                await handler.HandleAsync(job);
                await jobService.CompleteJobAsync(job.Id);
            }
            else
            {
                throw new Exception($"No handler found for job type: {job.JobType}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing job {JobId}", job.Id);
            await jobService.FailJobAsync(job.Id, ex.Message);
        }
    }

    private IJobHandler? GetJobHandler(IServiceProvider serviceProvider, string jobType)
    {
        return jobType switch
        {
            "SendEmail" => serviceProvider.GetService<SendEmailJobHandler>(),
            "ProcessPayment" => serviceProvider.GetService<ProcessPaymentJobHandler>(),
            "GenerateReport" => serviceProvider.GetService<GenerateReportJobHandler>(),
            _ => null
        };
    }
}

// Job Handler Interface
public interface IJobHandler
{
    Task HandleAsync(BackgroundJob job);
}

// Example: Send Email Job Handler
public class SendEmailJobHandler : IJobHandler
{
    private readonly CanPany.Application.Interfaces.Services.IEmailService _emailService;
    private readonly ILogger<SendEmailJobHandler> _logger;

    public SendEmailJobHandler(
        CanPany.Application.Interfaces.Services.IEmailService emailService,
        ILogger<SendEmailJobHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task HandleAsync(BackgroundJob job)
    {
        var payload = JsonSerializer.Deserialize<EmailPayload>(job.Payload);
        if (payload == null)
            throw new ArgumentException("Invalid email payload");

        await _emailService.SendEmailAsync(payload.To, payload.Subject, payload.Body, payload.IsHtml);
        _logger.LogInformation("Email sent to {To}", payload.To);
    }
}

public class EmailPayload
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;
}

// Example: Process Payment Job Handler
public class ProcessPaymentJobHandler : IJobHandler
{
    private readonly ILogger<ProcessPaymentJobHandler> _logger;

    public ProcessPaymentJobHandler(ILogger<ProcessPaymentJobHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(BackgroundJob job)
    {
        var payload = JsonSerializer.Deserialize<PaymentPayload>(job.Payload);
        if (payload == null)
            throw new ArgumentException("Invalid payment payload");

        // Implement payment processing logic here
        _logger.LogInformation("Processing payment for transaction {TransactionId}", payload.TransactionId);
        await Task.CompletedTask;
    }
}

public class PaymentPayload
{
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string UserId { get; set; } = string.Empty;
}

// Example: Generate Report Job Handler
public class GenerateReportJobHandler : IJobHandler
{
    private readonly ILogger<GenerateReportJobHandler> _logger;

    public GenerateReportJobHandler(ILogger<GenerateReportJobHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(BackgroundJob job)
    {
        var payload = JsonSerializer.Deserialize<ReportPayload>(job.Payload);
        if (payload == null)
            throw new ArgumentException("Invalid report payload");

        // Implement report generation logic here
        _logger.LogInformation("Generating report {ReportType} for user {UserId}", payload.ReportType, payload.UserId);
        await Task.CompletedTask;
    }
}

public class ReportPayload
{
    public string ReportType { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

