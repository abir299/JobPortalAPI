
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
            if (jobId <= 0)
                return BadRequest("Invalid job ID.");

            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
                return Unauthorized();

            var job = await _context.Jobs
                .AsNoTracking()
                .FirstOrDefaultAsync(j => j.Id == jobId);

            if (job == null)
                return NotFound("Job not found.");

            if (job.ApplicationDeadline.HasValue &&
                job.ApplicationDeadline.Value <= DateTime.UtcNow)
            {
                return BadRequest(
                    "The application deadline for this job has passed.");
            }

            var alreadyApplied = await _context.JobApplications
                .AnyAsync(a => a.JobId == jobId &&
                               a.JobSeekerId == userId);

            if (alreadyApplied)
            {
                return BadRequest(
                    "You have already applied for this job.");
            }

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
                application.Status,
                application.AppliedAt
            });
        }

        [Authorize(Roles = "JobSeeker")]
        [HttpGet("my-applications")]
        public async Task<IActionResult> GetMyApplications()
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
                return Unauthorized();

            var applications = await _context.JobApplications
                .AsNoTracking()
                .Where(a => a.JobSeekerId == userId)
                .OrderByDescending(a => a.AppliedAt)
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
            if (applicationId <= 0)
                return BadRequest("Invalid application ID.");

            var allowedStatuses = new[]
            {
                "Pending",
                "Reviewed",
                "Shortlisted",
                "Rejected",
                "Accepted"
            };

            if (string.IsNullOrWhiteSpace(request.Status) ||
                !allowedStatuses.Contains(
                    request.Status.Trim(),
                    StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message = "Invalid status. Allowed values: Pending, Reviewed, Shortlisted, Rejected, Accepted."
                });
            }

            var application = await _context.JobApplications
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null)
                return NotFound("Application not found.");

            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
                return Unauthorized();

            if (application.Job!.EmployerId != userId &&
                !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            application.Status = allowedStatuses.First(
                s => s.Equals(
                    request.Status.Trim(),
                    StringComparison.OrdinalIgnoreCase));

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
        public async Task<IActionResult> GetEmployerApplications(
            [FromQuery] int? jobId,
            [FromQuery] string? status)
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
                return Unauthorized();

            var allowedStatuses = new[]
            {
                "Pending",
                "Reviewed",
                "Shortlisted",
                "Rejected",
                "Accepted"
         };

            if (!string.IsNullOrWhiteSpace(status))
            {
                status = status.Trim();

                if (!allowedStatuses.Contains(
                    status, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest(new
                    {
                        message = "Invalid status filter."
                    });
                }

                status = allowedStatuses.First(s =>
                    s.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            if (jobId.HasValue && jobId.Value <= 0)
                return BadRequest("Invalid job ID.");

            var isAdmin = User.IsInRole("Admin");

            var query = _context.JobApplications
                .AsNoTracking()
                .Where(a => isAdmin || a.Job!.EmployerId == userId);

            if (jobId.HasValue)
                query = query.Where(a => a.JobId == jobId.Value);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(a => a.Status == status);

            var applications = await query
                .OrderByDescending(a => a.AppliedAt)
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
