using CanPany.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace CanPany.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BackgroundJobsController : ControllerBase
{
    private readonly IBackgroundJobService _jobService;
    private readonly ILogger<BackgroundJobsController> _logger;

    public BackgroundJobsController(
        IBackgroundJobService jobService,
        ILogger<BackgroundJobsController> logger)
    {
        _jobService = jobService;
        _logger = logger;
    }

    /// <summary>
    /// Enqueue an email job
    /// </summary>
    [HttpPost("email")]
    public async Task<IActionResult> EnqueueEmailJob([FromBody] EmailJobRequest request)
    {
        var payload = new
        {
            To = request.Email,
            Subject = request.Subject,
            Body = request.Body,
            IsHtml = request.IsHtml
        };

        var jobId = await _jobService.EnqueueJobAsync("SendEmail", payload);
        
        return Ok(new { JobId = jobId, Message = "Email queued for processing" });
    }

    /// <summary>
    /// Enqueue a payment processing job
    /// </summary>
    [HttpPost("payment")]
    public async Task<IActionResult> EnqueuePaymentJob([FromBody] PaymentJobRequest request)
    {
        var payload = new
        {
            TransactionId = request.TransactionId,
            Amount = request.Amount,
            UserId = request.UserId
        };

        var jobId = await _jobService.EnqueueJobAsync("ProcessPayment", payload);
        
        return Ok(new { JobId = jobId, Message = "Payment processing queued" });
    }

    /// <summary>
    /// Enqueue a report generation job
    /// </summary>
    [HttpPost("report")]
    public async Task<IActionResult> EnqueueReportJob([FromBody] ReportJobRequest request)
    {
        var payload = new
        {
            ReportType = request.ReportType,
            UserId = request.UserId,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        var jobId = await _jobService.EnqueueJobAsync("GenerateReport", payload);
        
        return Ok(new { JobId = jobId, Message = "Report generation queued" });
    }
}

public class EmailJobRequest
{
    public string Email { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;
}

public class PaymentJobRequest
{
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string UserId { get; set; } = string.Empty;
}

public class ReportJobRequest
{
    public string ReportType { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

