
using System.Security.Claims;
using JobPortalAPI.DTOs.Jobs;
using JobPortalAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPortalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly IJobService _jobService;

        public JobsController(IJobService jobService)
        {
            _jobService = jobService;
        }

        // GET: api/Jobs
        [HttpGet]
        public async Task<IActionResult> GetJobs(
            [FromQuery] string? search,
            [FromQuery] string? location,
            [FromQuery] string? category,
            [FromQuery] string? jobType,
            [FromQuery] string? experienceLevel,
            [FromQuery] decimal? minSalary,
            [FromQuery] decimal? maxSalary,
            [FromQuery] string? sortBy,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new
                {
                    message = "Page must be at least 1 and pageSize must be between 1 and 100."
                });
            }

            if (minSalary.HasValue && minSalary.Value < 0 ||
                maxSalary.HasValue && maxSalary.Value < 0 ||
                (minSalary.HasValue && maxSalary.HasValue &&
                 minSalary.Value > maxSalary.Value))
            {
                return BadRequest(new
                {
                    message = "Invalid salary range."
                });
            }

            try
            {
                var jobs = await _jobService.GetJobsAsync(
                    search,
                    location,
                    category,
                    jobType,
                    experienceLevel,
                    minSalary,
                    maxSalary,
                    sortBy,
                    page,
                    pageSize);

                return Ok(jobs);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/Jobs/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetJobById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid job ID." });
            }

            var job = await _jobService.GetJobByIdAsync(id);

            if (job == null)
            {
                return NotFound(new { message = "Job not found." });
            }

            return Ok(job);
        }

        // POST: api/Jobs

        [Authorize(Roles = "Employer,Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateJob(
            [FromBody] JobCreateDto request)
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
                return Unauthorized();

            try
            {
                var job = await _jobService.CreateJobAsync(
                    request, userId);

                return Ok(job);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Employer,Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateJob(
            int id,
            [FromBody] JobUpdateDto request)
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid job ID." });

            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
                return Unauthorized();

            try
            {
                var updated = await _jobService.UpdateJobAsync(
                    id,
                    request,
                    userId,
                    User.IsInRole("Admin"));

                if (!updated)
                    return NotFound(new { message = "Job not found." });

                return Ok(new { message = "Job updated successfully." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // DELETE: api/Jobs/1
        [Authorize(Roles = "Employer,Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid job ID." });
            }

            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var deleted = await _jobService.DeleteJobAsync(
                    id,
                    userId,
                    User.IsInRole("Admin"));

                if (!deleted)
                {
                    return NotFound(new { message = "Job not found." });
                }

                return Ok(new { message = "Job deleted successfully." });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }


        // GET: api/Jobs/dashboard
        [Authorize(Roles = "Employer,Admin")]
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
                return Unauthorized();

            var dashboard = await _jobService.GetEmployerDashboardAsync(
                userId,
                User.IsInRole("Admin"));

            return Ok(dashboard);
        }

    }
}
