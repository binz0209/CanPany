using CanPany.Application.Interfaces.Repositories;
using CanPany.Application.Interfaces.Services;
using CanPany.Application.Common.Constants;
using CanPany.Domain.Entities;
using CanPany.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace CanPany.Application.Services;

public class CVService : ICVService
{
    private readonly ICVRepository _repo;
    private readonly IGeminiService _geminiService;
    private readonly IJobService _jobService;
    private readonly ILogger<CVService> _logger;
    private readonly II18nService _i18n;

    public CVService(
        ICVRepository repo,
        IGeminiService geminiService,
        IJobService jobService,
        ILogger<CVService> logger,
        II18nService i18n)
    {
        _repo = repo;
        _geminiService = geminiService;
        _jobService = jobService;
        _logger = logger;
        _i18n = i18n;
    }

    public Task<CV?> GetByIdAsync(string id) => _repo.GetByIdAsync(id);
    public Task<IEnumerable<CV>> GetByCandidateIdAsync(string candidateId) => _repo.GetByCandidateIdAsync(candidateId);
    public Task<CV?> GetPrimaryCVAsync(string candidateId) => _repo.GetPrimaryCVAsync(candidateId);

    public async Task<CV> CreateAsync(CV cv)
    {
        try
        {
            _logger.LogInformation(_i18n.GetLogging(I18nKeys.Logging.CV.Create.Start), cv.CandidateId);

            if (string.IsNullOrWhiteSpace(cv.CandidateId))
                throw new ArgumentException(_i18n.GetValidation(I18nKeys.Validation.CV.CandidateIdRequired), nameof(cv.CandidateId));

            // If this is set as primary, unset other primary CVs
            if (cv.IsPrimary)
            {
                var existingPrimary = await _repo.GetPrimaryCVAsync(cv.CandidateId);
                if (existingPrimary != null)
                {
                    existingPrimary.IsPrimary = false;
                    await _repo.UpdateAsync(existingPrimary);
                }
            }

            cv.CreatedAt = DateTime.UtcNow;
            var created = await _repo.InsertAsync(cv);

            _logger.LogInformation(_i18n.GetLogging(I18nKeys.Logging.CV.Create.Complete), created.Id);
            return created;
        }
        catch (DomainException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating CV for candidate: {CandidateId}", cv.CandidateId);
            throw new InvalidDomainOperationException("CreateCV", _i18n.GetError(I18nKeys.Error.CV.CreateFailed, ex.Message), ex);
        }
    }

    public async Task<bool> UpdateAsync(string id, CV cv)
    {
        try
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                throw new EntityNotFoundException("CV", id);

            cv.Id = id;
            cv.CreatedAt = existing.CreatedAt;

            // If setting as primary, unset other primary CVs
            if (cv.IsPrimary && !existing.IsPrimary)
            {
                var currentPrimary = await _repo.GetPrimaryCVAsync(existing.CandidateId);
                if (currentPrimary != null && currentPrimary.Id != id)
                {
                    currentPrimary.IsPrimary = false;
                    await _repo.UpdateAsync(currentPrimary);
                }
            }

            return await _repo.UpdateAsync(cv);
        }
        catch (DomainException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating CV: {CVId}", id);
            throw new InvalidDomainOperationException("UpdateCV", _i18n.GetError(I18nKeys.Error.CV.UpdateFailed, ex.Message), ex);
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        try
        {
            var cv = await _repo.GetByIdAsync(id);
            if (cv == null)
                throw new EntityNotFoundException("CV", id);

            return await _repo.DeleteAsync(id);
        }
        catch (DomainException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting CV: {CVId}", id);
            throw new InvalidDomainOperationException("DeleteCV", _i18n.GetError(I18nKeys.Error.CV.DeleteFailed, ex.Message), ex);
        }
    }

    public async Task<bool> SetPrimaryAsync(string cvId, string candidateId)
    {
        try
        {
            var cv = await _repo.GetByIdAsync(cvId);
            if (cv == null || cv.CandidateId != candidateId)
                throw new EntityNotFoundException("CV", cvId);

            // Unset current primary
            var currentPrimary = await _repo.GetPrimaryCVAsync(candidateId);
            if (currentPrimary != null && currentPrimary.Id != cvId)
            {
                currentPrimary.IsPrimary = false;
                await _repo.UpdateAsync(currentPrimary);
            }

            // Set new primary
            cv.IsPrimary = true;
            return await _repo.UpdateAsync(cv);
        }
        catch (DomainException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting primary CV: {CVId}", cvId);
            throw new InvalidDomainOperationException("SetPrimaryCV", _i18n.GetError(I18nKeys.Error.CV.SetPrimaryFailed, ex.Message), ex);
        }
    }

    public async Task<CV> AnalyzeCVAsync(string cvId, string? jobId = null)
    {
        try
        {
            var cv = await _repo.GetByIdAsync(cvId);
            if (cv == null)
                throw new EntityNotFoundException("CV", cvId);

            if (string.IsNullOrWhiteSpace(cv.ExtractedText))
                throw new BusinessRuleViolationException("NoExtractedText", _i18n.GetError(I18nKeys.Error.CV.NoExtractedText));

            List<string> jobKeywords = new();
            if (!string.IsNullOrWhiteSpace(jobId))
            {
                var job = await _jobService.GetByIdAsync(jobId);
                if (job != null)
                {
                    // Extract keywords from job description
                    jobKeywords = await _geminiService.ExtractSkillsAsync(job.Description);
                }
            }

            // Analyze CV using Gemini
            var analysis = await _geminiService.AnalyzeCVAsync(cv.ExtractedText, jobKeywords);
            var skills = await _geminiService.ExtractSkillsFromCVAsync(cv.ExtractedText);

            // Update CV with analysis results
            cv.ATSScore = analysis.ATSScore;
            cv.ExtractedSkills = skills.Technical.Concat(skills.Soft).ToList();
            cv.Keywords = jobKeywords.Any() ? jobKeywords : analysis.MissingKeywords;
            cv.STARRewrittenContent = analysis.STARRewrittenSections;
            cv.AnalysisDate = DateTime.UtcNow;

            await _repo.UpdateAsync(cv);
            return cv;
        }
        catch (DomainException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing CV: {CVId}", cvId);
            throw new InvalidDomainOperationException("AnalyzeCV", _i18n.GetError(I18nKeys.Error.CV.AnalysisFailed, ex.Message), ex);
        }
    }

    public async Task<CV> GenerateCVForJobAsync(string candidateId, string jobId, string baseCVId)
    {
        try
        {
            var baseCV = await _repo.GetByIdAsync(baseCVId);
            if (baseCV == null || baseCV.CandidateId != candidateId)
                throw new EntityNotFoundException("CV", baseCVId);

            var job = await _jobService.GetByIdAsync(jobId);
            if (job == null)
                throw new EntityNotFoundException("Job", jobId);

            if (string.IsNullOrWhiteSpace(baseCV.ExtractedText))
                throw new BusinessRuleViolationException("NoExtractedText", _i18n.GetError(I18nKeys.Error.CV.NoExtractedText));

            // Use Gemini to generate tailored CV
            var prompt = $"Rewrite this CV to better match the job description:\n\nJob: {job.Title}\nDescription: {job.Description}\nRequired Skills: {string.Join(", ", job.RequiredSkills)}\n\nCV Content:\n{baseCV.ExtractedText}";
            var tailoredContent = await _geminiService.ChatAsync(prompt, baseCV.ExtractedText);

            // Create new CV version
            var newCV = new CV
            {
                CandidateId = candidateId,
                ExtractedText = tailoredContent,
                FileName = $"{baseCV.FileName}_tailored_{jobId}",
                FileType = baseCV.FileType,
                IsPrimary = false,
                Version = baseCV.Version + 1,
                CreatedAt = DateTime.UtcNow
            };

            return await CreateAsync(newCV);
        }
        catch (DomainException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating CV for job: {JobId}, Candidate: {CandidateId}", jobId, candidateId);
            throw new InvalidDomainOperationException("GenerateCVForJob", _i18n.GetError(I18nKeys.Error.CV.GenerateFailed, ex.Message), ex);
        }
    }
}




