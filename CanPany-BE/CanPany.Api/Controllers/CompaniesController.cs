using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CanPany.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _svc;

    public CompaniesController(ICompanyService svc)
    {
        _svc = svc;
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id) => Ok(await _svc.GetByIdAsync(id));

    [Authorize(Roles = "Company,Admin")]
    [HttpGet("my-company")]
    public async Task<IActionResult> GetMyCompany()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var company = await _svc.GetByUserIdAsync(userId);
        if (company == null)
            return NotFound();

        return Ok(company);
    }

    [Authorize(Roles = "Company")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Company dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        dto.UserId = userId;
        var created = await _svc.CreateAsync(dto);
        return Ok(created);
    }

    [Authorize(Roles = "Company,Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] Company dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var existing = await _svc.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        // Only company owner or admin can update
        if (existing.UserId != userId && !User.IsInRole("Admin"))
            return Forbid();

        var updated = await _svc.UpdateAsync(id, dto);
        return Ok(updated);
    }

    [Authorize(Roles = "Company")]
    [HttpPost("{id}/verification-request")]
    public async Task<IActionResult> RequestVerification(string id, [FromBody] List<string> documentUrls)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var company = await _svc.GetByIdAsync(id);
        if (company == null || company.UserId != userId)
            return Forbid();

        var result = await _svc.RequestVerificationAsync(id, documentUrls);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/approve-verification")]
    public async Task<IActionResult> ApproveVerification(string id)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(adminId))
            return Unauthorized();

        var result = await _svc.ApproveVerificationAsync(id, adminId);
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("pending-verifications")]
    public async Task<IActionResult> GetPendingVerifications()
        => Ok(await _svc.GetPendingVerificationsAsync());
}

