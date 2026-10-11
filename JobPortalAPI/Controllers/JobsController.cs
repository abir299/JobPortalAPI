using JobPortalAPI.DTOs.Jobs;
using JobPortalAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobPortalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly IJobService _jobService;

        public JobsController(IJobService jobService)
        {
            _jobService = jobService;
        }

        [HttpGet]
        public async Task<IActionResult> GetJobs(
            string? search,
            string? location,
            string? category,
            int page = 1,
            int pageSize = 10)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
                return BadRequest("Invalid pagination values.");

            var jobs = await _jobService.GetJobsAsync(
                search, location, category, page, pageSize);

            return Ok(jobs);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetJob(int id)
        {
            var job = await _jobService.GetJobByIdAsync(id);

            if (job == null)
                return NotFound();

            return Ok(job);
        }

        [Authorize(Roles = "Employer,Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateJob(JobCreateDto request)
        {
            var employerId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var job = await _jobService.CreateJobAsync(
                request, employerId);

            return Ok(job);
        }

        [Authorize(Roles = "Employer,Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateJob(
            int id, JobUpdateDto request)
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var updated = await _jobService.UpdateJobAsync(
                    id, request, userId, User.IsInRole("Admin"));

                if (!updated)
                    return NotFound();

                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [Authorize(Roles = "Employer,Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                var deleted = await _jobService.DeleteJobAsync(
                    id, userId, User.IsInRole("Admin"));

                if (!deleted)
                    return NotFound();

                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }
}
