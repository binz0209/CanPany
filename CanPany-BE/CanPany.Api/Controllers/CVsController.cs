using CanPany.Application.Interfaces.Services;
using CanPany.Domain.Entities;
using CanPany.Application.Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CanPany.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CVsController : ControllerBase
{
    private readonly ICVService _cvService;
    private readonly IImageUploadService _imageUploadService;
    private readonly II18nService _i18n;

    public CVsController(ICVService cvService, IImageUploadService imageUploadService, II18nService i18n)
    {
        _cvService = cvService;
        _imageUploadService = imageUploadService;
        _i18n = i18n;
    }

    private string? GetUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.FindFirst("sub")?.Value
           ?? User.FindFirst("userId")?.Value;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var cv = await _cvService.GetByIdAsync(id);
            if (cv == null)
                return NotFound(new { message = _i18n.GetError(I18nKeys.Error.CV.NotFound) });

            var userId = GetUserId();
            if (cv.CandidateId != userId && !User.IsInRole("Admin"))
                return Forbid();

            return Ok(cv);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _i18n.GetError(I18nKeys.Error.Common.InternalServerError) });
        }
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyCVs()
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var cvs = await _cvService.GetByCandidateIdAsync(userId);
            return Ok(cvs);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _i18n.GetError(I18nKeys.Error.Common.InternalServerError) });
        }
    }

    [HttpGet("primary")]
    public async Task<IActionResult> GetPrimaryCV()
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var cv = await _cvService.GetPrimaryCVAsync(userId);
            if (cv == null)
                return NotFound(new { message = _i18n.GetError(I18nKeys.Error.CV.NotFound) });

            return Ok(cv);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _i18n.GetError(I18nKeys.Error.Common.InternalServerError) });
        }
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [ApiExplorerSettings(IgnoreApi = true)] // Tạm thời ẩn khỏi Swagger để tránh lỗi
    public async Task<IActionResult> Create([FromForm] IFormFile file)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (file == null || file.Length == 0)
                return BadRequest(new { message = _i18n.GetValidation(I18nKeys.Validation.CV.FileRequired) });

            // Upload file using image upload service (Cloudinary supports PDF/DOCX)
            var fileUrl = await _imageUploadService.UploadImageAsync(
                file.OpenReadStream(), 
                file.FileName, 
                "cvs"
            );
            
            // Extract text from CV (simplified - in production, use proper PDF/DOCX parser)
            // For now, we'll store the file URL and extract text later via background job

            var cv = new CV
            {
                CandidateId = userId,
                FileUrl = fileUrl,
                FileName = file.FileName,
                FileSize = file.Length,
                FileType = file.ContentType.Contains("pdf") ? "pdf" : "docx",
                CreatedAt = DateTime.UtcNow
            };

            var created = await _cvService.CreateAsync(cv);
            return Ok(created);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _i18n.GetError(I18nKeys.Error.CV.CreateFailed, ex.Message) });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] CV cv)
    {
        try
        {
            var userId = GetUserId();
            var existing = await _cvService.GetByIdAsync(id);
            
            if (existing == null)
                return NotFound(new { message = _i18n.GetError(I18nKeys.Error.CV.NotFound) });

            if (existing.CandidateId != userId && !User.IsInRole("Admin"))
                return Forbid();

            var updated = await _cvService.UpdateAsync(id, cv);
            if (!updated)
                return BadRequest(new { message = _i18n.GetError(I18nKeys.Error.CV.UpdateFailed) });

            return Ok(await _cvService.GetByIdAsync(id));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _i18n.GetError(I18nKeys.Error.CV.UpdateFailed, ex.Message) });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var userId = GetUserId();
            var cv = await _cvService.GetByIdAsync(id);
            
            if (cv == null)
                return NotFound(new { message = _i18n.GetError(I18nKeys.Error.CV.NotFound) });

            if (cv.CandidateId != userId && !User.IsInRole("Admin"))
                return Forbid();

            var deleted = await _cvService.DeleteAsync(id);
            if (!deleted)
                return BadRequest(new { message = _i18n.GetError(I18nKeys.Error.CV.DeleteFailed) });

            return Ok(new { message = _i18n.GetSuccess(I18nKeys.Success.CV.Delete) });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _i18n.GetError(I18nKeys.Error.CV.DeleteFailed, ex.Message) });
        }
    }

    [HttpPost("{id}/set-primary")]
    public async Task<IActionResult> SetPrimary(string id)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var updated = await _cvService.SetPrimaryAsync(id, userId);
            if (!updated)
                return BadRequest(new { message = _i18n.GetError(I18nKeys.Error.CV.SetPrimaryFailed) });

            return Ok(new { message = _i18n.GetSuccess(I18nKeys.Success.CV.SetPrimary) });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _i18n.GetError(I18nKeys.Error.CV.SetPrimaryFailed, ex.Message) });
        }
    }

    [HttpPost("{id}/analyze")]
    public async Task<IActionResult> Analyze(string id, [FromQuery] string? jobId = null)
    {
        try
        {
            var userId = GetUserId();
            var cv = await _cvService.GetByIdAsync(id);
            
            if (cv == null)
                return NotFound(new { message = _i18n.GetError(I18nKeys.Error.CV.NotFound) });

            if (cv.CandidateId != userId && !User.IsInRole("Admin"))
                return Forbid();

            var analyzed = await _cvService.AnalyzeCVAsync(id, jobId);
            return Ok(analyzed);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _i18n.GetError(I18nKeys.Error.CV.AnalysisFailed, ex.Message) });
        }
    }

    [HttpPost("generate-for-job")]
    public async Task<IActionResult> GenerateForJob([FromBody] GenerateCVRequest request)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var generated = await _cvService.GenerateCVForJobAsync(userId, request.JobId, request.BaseCVId);
            return Ok(generated);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _i18n.GetError(I18nKeys.Error.CV.GenerateFailed, ex.Message) });
        }
    }

    public record GenerateCVRequest(string JobId, string BaseCVId);
}

