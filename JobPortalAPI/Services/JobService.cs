using JobPortalAPI.Data;
using JobPortalAPI.DTOs.Jobs;
using JobPortalAPI.Interfaces;
using JobPortalAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace JobPortalAPI.Services
{
    public class JobService : IJobService
    {
        private readonly JobPortalDbContext _context;

        public JobService(JobPortalDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetJobsAsync(
            string? search,
            string? location,
            string? category,
            int page,
            int pageSize)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
                throw new ArgumentException("Invalid pagination values.");

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

            return new { total, page, pageSize, jobs };
        }

        public async Task<object?> GetJobByIdAsync(int id)
        {
            return await _context.Jobs
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
        }

        public async Task<object> CreateJobAsync(
            JobCreateDto request,
            int employerId)
        {
            var job = new Job
            {
                Title = request.Title.Trim(),
                Description = request.Description.Trim(),
                CompanyName = request.CompanyName.Trim(),
                Location = request.Location.Trim(),
                Salary = request.Salary,
                Category = request.Category.Trim(),
                EmployerId = employerId,
                PostedAt = DateTime.UtcNow
            };

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            return new
            {
                job.Id,
                job.Title,
                job.Description,
                job.CompanyName,
                job.Location,
                job.Salary,
                job.Category,
                job.PostedAt,
                job.EmployerId
            };
        }

        public async Task<bool> UpdateJobAsync(
            int id,
            JobUpdateDto request,
            int userId,
            bool isAdmin)
        {
            var job = await _context.Jobs.FindAsync(id);

            if (job == null)
                return false;

            if (job.EmployerId != userId && !isAdmin)
                throw new UnauthorizedAccessException(
                    "You cannot update this job.");

            job.Title = request.Title.Trim();
            job.Description = request.Description.Trim();
            job.CompanyName = request.CompanyName.Trim();
            job.Location = request.Location.Trim();
            job.Salary = request.Salary;
            job.Category = request.Category.Trim();

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteJobAsync(
            int id,
            int userId,
            bool isAdmin)
        {
            var job = await _context.Jobs.FindAsync(id);

            if (job == null)
                return false;

            if (job.EmployerId != userId && !isAdmin)
                throw new UnauthorizedAccessException(
                    "You cannot delete this job.");

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
