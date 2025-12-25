using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanPany.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;

    public JobsController(IJobService jobService)
    {
        _jobService = jobService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _jobService.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var job = await _jobService.GetByIdAsync(id);
        if (job == null) return NotFound();
        return Ok(job);
    }

    [HttpGet("company/{companyId}")]
    public async Task<IActionResult> GetByCompany(string companyId)
        => Ok(await _jobService.GetByCompanyIdAsync(companyId));

    [HttpGet("open")]
    public async Task<IActionResult> GetOpen() => Ok(await _jobService.GetOpenJobsAsync());

    [HttpGet("recommended")]
    public async Task<IActionResult> GetRecommended([FromQuery] string candidateId, [FromQuery] int limit = 100)
    {
        if (string.IsNullOrWhiteSpace(candidateId))
        {
            return BadRequest("candidateId is required");
        }

        var items = await _jobService.GetRecommendedJobsAsync(candidateId, limit);
        var result = items.Select(x => new
        {
            job = x.Job,
            similarity = x.Similarity
        });
        return Ok(result);
    }

    [Authorize(Roles = "Company,Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Job job)
    {
        var created = await _jobService.CreateAsync(job);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = "Company,Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] Job job)
    {
        var ok = await _jobService.UpdateAsync(id, job);
        return ok ? NoContent() : NotFound();
    }

    [Authorize(Roles = "Company,Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var ok = await _jobService.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }

    [Authorize(Roles = "Company,Admin")]
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] StatusDto dto)
    {
        var updated = await _jobService.UpdateStatusAsync(id, dto.Status);
        return updated != null ? Ok(updated) : NotFound();
    }

    public record StatusDto(string Status);
}

