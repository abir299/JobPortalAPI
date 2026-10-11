
using JobPortalAPI.Data;
using JobPortalAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JobPortalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly JobPortalDbContext _context;

        public JobsController(JobPortalDbContext context)
        {
            _context = context;
        }

        // Public: search and view jobs
        [HttpGet]
        public async Task<IActionResult> GetJobs(
            string? search, string? location, string? category,
            int page = 1, int pageSize = 10)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
                return BadRequest("Invalid pagination values.");

            var query = _context.Jobs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(j =>
                    j.Title.Contains(search) ||
                    j.Description.Contains(search) ||
                    j.CompanyName.Contains(search));

            if (!string.IsNullOrWhiteSpace(location))
                query = query.Where(j => j.Location.Contains(location));

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(j => j.Category == category);

            var total = await query.CountAsync();

            var jobs = await query
                .OrderByDescending(j => j.PostedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(j => new
                {
                    j.Id,
                    j.Title,
                    j.Description,
                    j.CompanyName,
                    j.Location,
                    j.Salary,
                    j.Category,
                    j.PostedAt,
                    j.EmployerId
                })
                .ToListAsync();

            return Ok(new { total, page, pageSize, jobs });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetJob(int id)
        {
            var job = await _context.Jobs
                .AsNoTracking()
                .Where(j => j.Id == id)
                .Select(j => new
                {
                    j.Id,
                    j.Title,
                    j.Description,
                    j.CompanyName,
                    j.Location,
                    j.Salary,
                    j.Category,
                    j.PostedAt,
                    j.EmployerId
                })
                .FirstOrDefaultAsync();

            return job == null ? NotFound() : Ok(job);
        }

        // Employer only
        [Authorize(Roles = "Employer,Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateJob(Job job)
        {
            if (string.IsNullOrWhiteSpace(job.Title) ||
                string.IsNullOrWhiteSpace(job.Description) ||
                string.IsNullOrWhiteSpace(job.CompanyName) ||
                string.IsNullOrWhiteSpace(job.Location) ||
                string.IsNullOrWhiteSpace(job.Category) ||
                job.Salary < 0)
                return BadRequest("Valid job details are required.");

            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            job.Id = 0;
            job.EmployerId = userId;
            job.PostedAt = DateTime.UtcNow;

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetJob),
                new { id = job.Id }, job);
        }

        [Authorize(Roles = "Employer,Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateJob(int id, Job updatedJob)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null) return NotFound();

            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (job.EmployerId != userId &&
                !User.IsInRole("Admin"))
                return Forbid();

            if (string.IsNullOrWhiteSpace(updatedJob.Title) ||
                string.IsNullOrWhiteSpace(updatedJob.Description) ||
                string.IsNullOrWhiteSpace(updatedJob.CompanyName) ||
                string.IsNullOrWhiteSpace(updatedJob.Location) ||
                string.IsNullOrWhiteSpace(updatedJob.Category) ||
                updatedJob.Salary < 0)
                return BadRequest("Valid job details are required.");

            job.Title = updatedJob.Title;
            job.Description = updatedJob.Description;
            job.CompanyName = updatedJob.CompanyName;
            job.Location = updatedJob.Location;
            job.Salary = updatedJob.Salary;
            job.Category = updatedJob.Category;

            await _context.SaveChangesAsync();
            return Ok(job);
        }

        [Authorize(Roles = "Employer,Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null) return NotFound();

            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (job.EmployerId != userId &&
                !User.IsInRole("Admin"))
                return Forbid();

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
