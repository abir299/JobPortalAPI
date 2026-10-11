using JobPortalAPI.DTOs.Jobs;

namespace JobPortalAPI.Interfaces
{
    public interface IJobService
    {
        Task<object> GetJobsAsync(
            string? search,
            string? location,
            string? category,
            int page,
            int pageSize);

        Task<object?> GetJobByIdAsync(int id);

        Task<object> CreateJobAsync(
            JobCreateDto request,
            int employerId);

        Task<bool> UpdateJobAsync(
            int id,
            JobUpdateDto request,
            int userId,
            bool isAdmin);

        Task<bool> DeleteJobAsync(
            int id,
            int userId,
            bool isAdmin);
    }
}
