using JobPortalAPI.Data;
using JobPortalAPI.DTOs.Applications;
using JobPortalAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JobPortalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobApplicationsController : ControllerBase
    {
        private readonly JobPortalDbContext _context;

        public JobApplicationsController(JobPortalDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "JobSeeker")]
        [HttpPost("{jobId:int}")]
        public async Task<IActionResult> Apply(int jobId)
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var jobExists = await _context.Jobs
                .AnyAsync(j => j.Id == jobId);

            if (!jobExists)
                return NotFound("Job not found.");

            var alreadyApplied = await _context.JobApplications
                .AnyAsync(a => a.JobId == jobId &&
                               a.JobSeekerId == userId);

            if (alreadyApplied)
                return BadRequest(
                    "You have already applied for this job.");

            var application = new JobApplication
            {
                JobId = jobId,
                JobSeekerId = userId,
                AppliedAt = DateTime.UtcNow,
                Status = "Pending"
            };

            _context.JobApplications.Add(application);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Application submitted successfully.",
                application.Id,
                application.JobId,
                application.Status
            });
        }

        [Authorize(Roles = "JobSeeker")]
        [HttpGet("my-applications")]
        public async Task<IActionResult> GetMyApplications()
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var applications = await _context.JobApplications
                .AsNoTracking()
                .Where(a => a.JobSeekerId == userId)
                .Select(a => new JobApplicationResponseDto
                {
                    Id = a.Id,
                    JobId = a.JobId,
                    JobTitle = a.Job!.Title,
                    JobSeekerId = a.JobSeekerId,
                    JobSeekerName = a.JobSeeker!.FullName,
                    AppliedAt = a.AppliedAt,
                    Status = a.Status
                })
                .ToListAsync();

            return Ok(applications);
        }

        [Authorize(Roles = "Employer,Admin")]
        [HttpPut("{applicationId:int}/status")]
        public async Task<IActionResult> UpdateStatus(
            int applicationId,
            [FromBody] UpdateApplicationStatusDto request)
        {
            var application = await _context.JobApplications
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null)
                return NotFound("Application not found.");

            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (application.Job!.EmployerId != userId &&
                !User.IsInRole("Admin"))
                return Forbid();

            application.Status = request.Status;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Application status updated.",
                application.Id,
                application.Status
            });
        }

        [Authorize(Roles = "Employer,Admin")]
        [HttpGet("employer-applications")]
        public async Task<IActionResult> GetEmployerApplications()
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var applications = await _context.JobApplications
                .AsNoTracking()
                .Where(a => User.IsInRole("Admin") ||
                            a.Job!.EmployerId == userId)
                .Select(a => new JobApplicationResponseDto
                {
                    Id = a.Id,
                    JobId = a.JobId,
                    JobTitle = a.Job!.Title,
                    JobSeekerId = a.JobSeekerId,
                    JobSeekerName = a.JobSeeker!.FullName,
                    AppliedAt = a.AppliedAt,
                    Status = a.Status
                })
                .ToListAsync();

            return Ok(applications);
        }
    }
}
