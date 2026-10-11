namespace JobPortalAPI.DTOs.Applications
{
    public class JobApplicationResponseDto
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public int JobSeekerId { get; set; }
        public string JobSeekerName { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}