
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
            string? jobType,
            string? experienceLevel,
            decimal? minSalary,
            decimal? maxSalary,
            string? sortBy,
            int page,
            int pageSize)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
                throw new ArgumentException("Invalid pagination values.");

            if ((minSalary.HasValue && minSalary.Value < 0) ||
                (maxSalary.HasValue && maxSalary.Value < 0) ||
                (minSalary.HasValue && maxSalary.HasValue &&
                 minSalary.Value > maxSalary.Value))
            {
                throw new ArgumentException("Invalid salary range.");
            }

            var now = DateTime.UtcNow;

            var query = _context.Jobs
                .AsNoTracking()
                .Where(j =>
                    !j.ApplicationDeadline.HasValue ||
                    j.ApplicationDeadline.Value > now)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();

                query = query.Where(j =>
                    j.Title.Contains(term) ||
                    j.Description.Contains(term) ||
                    j.CompanyName.Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                var locationTerm = location.Trim();

                query = query.Where(j =>
                    j.Location.Contains(locationTerm));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                var categoryTerm = category.Trim();
                query = query.Where(j => j.Category == categoryTerm);
            }

            if (!string.IsNullOrWhiteSpace(jobType))
            {
                var type = jobType.Trim();
                query = query.Where(j => j.JobType == type);
            }

            if (!string.IsNullOrWhiteSpace(experienceLevel))
            {
                var level = experienceLevel.Trim();
                query = query.Where(j => j.ExperienceLevel == level);
            }

            if (minSalary.HasValue)
                query = query.Where(j => j.Salary >= minSalary.Value);

            if (maxSalary.HasValue)
                query = query.Where(j => j.Salary <= maxSalary.Value);

            var total = await query.CountAsync();

            query = sortBy?.Trim().ToLowerInvariant() switch
            {
                "salary_asc" => query
                    .OrderBy(j => j.Salary)
                    .ThenBy(j => j.Id),

                "salary_desc" => query
                    .OrderByDescending(j => j.Salary)
                    .ThenBy(j => j.Id),

                "oldest" => query
                    .OrderBy(j => j.PostedAt)
                    .ThenBy(j => j.Id),

                _ => query
                    .OrderByDescending(j => j.PostedAt)
                    .ThenByDescending(j => j.Id)
            };

            var jobs = await query
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
                    j.JobType,
                    j.ExperienceLevel,
                    j.ApplicationDeadline,
                    j.PostedAt,
                    j.EmployerId
                })
                .ToListAsync();

            return new
            {
                total,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(
                    total / (double)pageSize),
                jobs
            };
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
                    j.JobType,
                    j.ExperienceLevel,
                    j.ApplicationDeadline,
                    j.PostedAt,
                    j.EmployerId,

                    IsExpired = j.ApplicationDeadline.HasValue &&
                       j.ApplicationDeadline.Value <= DateTime.UtcNow,

                    ApplicationCount = _context.JobApplications
                        .Count(a => a.JobId == j.Id),

                    PendingApplications = _context.JobApplications
                        .Count(a => a.JobId == j.Id &&
                                    a.Status == "Pending"),

                    ReviewedApplications = _context.JobApplications
                        .Count(a => a.JobId == j.Id &&
                                    a.Status == "Reviewed"),

                    ShortlistedApplications = _context.JobApplications
                        .Count(a => a.JobId == j.Id &&
                                    a.Status == "Shortlisted"),

                    AcceptedApplications = _context.JobApplications
                        .Count(a => a.JobId == j.Id &&
                                    a.Status == "Accepted"),

                    RejectedApplications = _context.JobApplications
                        .Count(a => a.JobId == j.Id &&
                                    a.Status == "Rejected")
                })
                .FirstOrDefaultAsync();
        }


        public async Task<object> GetEmployerDashboardAsync(
            int userId,
            bool isAdmin)
        {
            var now = DateTime.UtcNow;

            var jobsQuery = _context.Jobs
                .AsNoTracking()
                .AsQueryable();

            if (!isAdmin)
            {
                jobsQuery = jobsQuery.Where(
                    j => j.EmployerId == userId);
            }

            var jobCount = await jobsQuery.CountAsync();

            var activeJobs = await jobsQuery.CountAsync(
                j => !j.ApplicationDeadline.HasValue ||
                     j.ApplicationDeadline.Value > now);

            var expiredJobs = await jobsQuery.CountAsync(
                j => j.ApplicationDeadline.HasValue &&
                     j.ApplicationDeadline.Value <= now);

            var jobIdsQuery = jobsQuery.Select(j => j.Id);

            var applicationsQuery = _context.JobApplications
                .AsNoTracking()
                .Where(a => jobIdsQuery.Contains(a.JobId));

            var totalApplications =
                await applicationsQuery.CountAsync();

            var pendingApplications =
                await applicationsQuery.CountAsync(
                    a => a.Status == "Pending");

            var reviewedApplications =
                await applicationsQuery.CountAsync(
                    a => a.Status == "Reviewed");

            var shortlistedApplications =
                await applicationsQuery.CountAsync(
                    a => a.Status == "Shortlisted");

            var acceptedApplications =
                await applicationsQuery.CountAsync(
                    a => a.Status == "Accepted");

            var rejectedApplications =
                await applicationsQuery.CountAsync(
                    a => a.Status == "Rejected");

            var jobs = await jobsQuery
                .OrderByDescending(j => j.PostedAt)
                .Select(j => new
                {
                    j.Id,
                    j.Title,
                    j.CompanyName,
                    j.Location,
                    j.JobType,
                    j.ExperienceLevel,
                    j.PostedAt,
                    j.ApplicationDeadline,

                    IsExpired = j.ApplicationDeadline.HasValue &&
                                j.ApplicationDeadline.Value <= now,

                    ApplicationCount = _context.JobApplications
                        .Count(a => a.JobId == j.Id),

                    PendingApplications = _context.JobApplications
                        .Count(a => a.JobId == j.Id &&
                                    a.Status == "Pending")
                })
                .ToListAsync();

            return new
            {
                IsAdminDashboard = isAdmin,
                TotalJobs = jobCount,
                ActiveJobs = activeJobs,
                ExpiredJobs = expiredJobs,
                TotalApplications = totalApplications,
                PendingApplications = pendingApplications,
                ReviewedApplications = reviewedApplications,
                ShortlistedApplications = shortlistedApplications,
                AcceptedApplications = acceptedApplications,
                RejectedApplications = rejectedApplications,
                Jobs = jobs
            };
        }


        public async Task<object> CreateJobAsync(
            JobCreateDto request,
            int employerId)
        {
            if (request.ApplicationDeadline.HasValue &&
                request.ApplicationDeadline.Value <= DateTime.UtcNow)
            {
                throw new ArgumentException(
                    "Application deadline must be in the future.");
            }

            var job = new Job
            {
                Title = request.Title.Trim(),
                Description = request.Description.Trim(),
                CompanyName = request.CompanyName.Trim(),
                Location = request.Location.Trim(),
                Salary = request.Salary,
                Category = request.Category.Trim(),
                JobType = request.JobType,
                ExperienceLevel = request.ExperienceLevel,
                ApplicationDeadline = request.ApplicationDeadline,
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
                job.JobType,
                job.ExperienceLevel,
                job.ApplicationDeadline,
                IsExpired = false,
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
            if (request.ApplicationDeadline.HasValue &&
                request.ApplicationDeadline.Value <= DateTime.UtcNow)
            {
                throw new ArgumentException(
                    "Application deadline must be in the future.");
            }

            var job = await _context.Jobs.FindAsync(id);

            if (job == null)
                return false;

            if (job.EmployerId != userId && !isAdmin)
            {
                throw new UnauthorizedAccessException(
                    "You cannot update this job.");
            }

            job.Title = request.Title.Trim();
            job.Description = request.Description.Trim();
            job.CompanyName = request.CompanyName.Trim();
            job.Location = request.Location.Trim();
            job.Salary = request.Salary;
            job.Category = request.Category.Trim();
            job.JobType = request.JobType;
            job.ExperienceLevel = request.ExperienceLevel;
            job.ApplicationDeadline = request.ApplicationDeadline;

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
            {
                throw new UnauthorizedAccessException(
                    "You cannot delete this job.");
            }

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
