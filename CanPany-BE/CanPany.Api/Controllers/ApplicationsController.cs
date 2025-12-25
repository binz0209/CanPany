using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanPany.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _appService;

    public ApplicationsController(IApplicationService appService)
    {
        _appService = appService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var app = await _appService.GetByIdAsync(id);
        if (app == null) return NotFound();
        return Ok(app);
    }

    [HttpGet("job/{jobId}")]
    public async Task<IActionResult> GetByJob(string jobId)
        => Ok(await _appService.GetByJobIdAsync(jobId));

    [HttpGet("candidate/{candidateId}")]
    public async Task<IActionResult> GetByCandidate(string candidateId)
        => Ok(await _appService.GetByCandidateIdAsync(candidateId));

    [Authorize(Roles = "Candidate,Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] JobApplication app)
    {
        var created = await _appService.CreateAsync(app);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = "Company,Admin")]
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] StatusDto dto)
    {
        var ok = await _appService.UpdateStatusAsync(id, dto.Status);
        return ok ? NoContent() : NotFound();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var ok = await _appService.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }

    public record StatusDto(string Status);
}

